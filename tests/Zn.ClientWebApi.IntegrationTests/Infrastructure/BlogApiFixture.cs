using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zn.Domain.Authorization;
using Zn.Domain.Entity;

namespace Zn.ClientWebApi.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Collection fixture for Blog and Category integration tests.
    /// Each fixture instance gets its own isolated LocalDB database (ZnApp_Test_{guid}).
    /// Admin token is cached after the first acquisition so tests do not re-authenticate on every call.
    /// </summary>
    public sealed class BlogApiFixture : IAsyncLifetime
    {
        public AuthWebApplicationFactory Factory { get; } = new();

        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // Cached admin access token; lazy-initialized by EnsureAdminTokenAsync().
        private string? _adminAccessToken;
        private readonly object _adminTokenLock = new();

        // Cached manager access token; lazy-initialized by CreateManagerClientAsync().
        private string? _managerAccessToken;

        public HttpClient CreateClient() =>
            Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        /// <summary>
        /// Returns an HttpClient pre-configured with a valid admin Bearer token.
        /// The token is fetched once and reused for the lifetime of the fixture.
        /// </summary>
        public async Task<HttpClient> CreateAdminClientAsync()
        {
            string token = await EnsureAdminTokenAsync();
            HttpClient client = CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        /// <summary>
        /// Registers a new user, logs in, and returns an HttpClient bearing that user's token.
        /// Useful for "non-admin authenticated user" scenarios.
        /// </summary>
        public async Task<(HttpClient Client, string UserId, string AccessToken)> CreateUserClientAsync(string prefix = "user")
        {
            HttpClient anon = CreateClient();
            string email = UniqueEmail(prefix);
            const string password = "Valid@1234";

            HttpResponseMessage reg = await anon.PostAsJsonAsync("/api/auth/register", new
            {
                FirstName = "Test",
                LastName = "User",
                Email = email,
                Password = password,
                ImageUrl = "https://example.com/avatar.png"
            }, JsonOptions);

            string regBody = await reg.Content.ReadAsStringAsync();
            if (!reg.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"User registration failed ({reg.StatusCode}): {regBody}");
            }

            // Parse the user id from the registration response
            var regDoc = JsonDocument.Parse(regBody);
            string userId = regDoc.RootElement.GetProperty("id").GetString()!;

            HttpResponseMessage login = await anon.PostAsJsonAsync("/api/auth/login", new
            {
                Email = email,
                Password = password
            }, JsonOptions);

            if (!login.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"User login failed ({login.StatusCode})");
            }

            var tokens = await login.Content.ReadFromJsonAsync<TokenPairDto>(JsonOptions);
            string accessToken = tokens!.AccessToken;

            HttpClient userClient = CreateClient();
            userClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            return (userClient, userId, accessToken);
        }

        /// <summary>
        /// Creates a new Manager-role user and returns an HttpClient bearing that user's token.
        /// The Manager role is created in the test DB if it does not exist yet.
        /// A single manager identity is cached for the lifetime of the fixture.
        /// </summary>
        public async Task<HttpClient> CreateManagerClientAsync(string prefix = "manager")
        {
            if (_managerAccessToken is not null)
            {
                HttpClient cached = CreateClient();
                cached.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _managerAccessToken);
                return cached;
            }

            // Create the Manager role if it doesn't exist, then create a user in that role.
            using IServiceScope scope = Factory.Services.CreateScope();
            IServiceProvider provider = scope.ServiceProvider;
            var roleManager = provider.GetRequiredService<RoleManager<Role>>();
            var userManager = provider.GetRequiredService<UserManager<User>>();

            if (!await roleManager.RoleExistsAsync(RoleNames.Manager))
            {
                await roleManager.CreateAsync(new Role { Name = RoleNames.Manager });
            }

            string email = UniqueEmail(prefix);
            const string password = "Manager@1234";
            var managerUser = new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = "Test",
                LastName = "Manager",
                ImageUrl = "https://www.gravatar.com/avatar/?d=mp"
            };

            IdentityResult createResult = await userManager.CreateAsync(managerUser, password);
            if (!createResult.Succeeded)
            {
                var msgs = new System.Collections.Generic.List<string>();
                foreach (var err in createResult.Errors) msgs.Add(err.Description);
                throw new InvalidOperationException(
                    "Failed to create manager user: " + string.Join("; ", msgs));
            }

            await userManager.AddToRoleAsync(managerUser, RoleNames.Manager);

            // Login to get token
            HttpClient anon = CreateClient();
            HttpResponseMessage login = await anon.PostAsJsonAsync("/api/auth/login",
                new { Email = email, Password = password }, JsonOptions);

            if (!login.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Manager login failed ({login.StatusCode})");
            }

            var tokens = await login.Content.ReadFromJsonAsync<TokenPairDto>(JsonOptions);
            _managerAccessToken = tokens!.AccessToken;

            HttpClient client = CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _managerAccessToken);
            return client;
        }

        public async Task InitializeAsync()
        {
            await Factory.InitializeAsync();
            // Ensure admin user exists in the test DB.
            // IdentityDataSeeder runs during WebApplicationFactory host startup (before MigrateAsync),
            // so the admin user may not have been seeded into the isolated test DB.
            // We re-seed it explicitly here, after MigrateAsync has run.
            await EnsureAdminUserSeededAsync();
        }

        public async Task DisposeAsync() => await Factory.DisposeAsync();

        public static string UniqueEmail(string prefix = "testuser") =>
            $"{prefix}-{Guid.NewGuid():N}@integration.test";

        // ------------------------------------------------------------------
        // Internal helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Ensures the admin user exists in the test DB by creating it via UserManager if absent.
        /// This is necessary because IdentityDataSeeder runs during host startup (before MigrateAsync),
        /// so it cannot seed into the freshly-created isolated test database.
        /// </summary>
        private async Task EnsureAdminUserSeededAsync()
        {
            using IServiceScope scope = Factory.Services.CreateScope();
            IServiceProvider provider = scope.ServiceProvider;

            var config = provider.GetRequiredService<IConfiguration>();
            string? adminEmail = config["AdminUser:Email"];
            string? adminPassword = config["AdminUser:Password"];
            string adminFirstName = config["AdminUser:FirstName"] ?? "System";
            string adminLastName = config["AdminUser:LastName"] ?? "Administrator";

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "AdminUser:Email / AdminUser:Password not configured. " +
                    "Check appsettings.Development.json.");
            }

            var roleManager = provider.GetRequiredService<RoleManager<Role>>();
            var userManager = provider.GetRequiredService<UserManager<User>>();

            // Ensure all application roles exist (Admin, Manager, User)
            foreach (string roleName in RoleNames.All)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role { Name = roleName });
                }
            }

            // Ensure the admin user exists
            User? existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin is null)
            {
                var admin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = adminFirstName,
                    LastName = adminLastName,
                    ImageUrl = "https://www.gravatar.com/avatar/?d=mp"
                };

                IdentityResult createResult = await userManager.CreateAsync(admin, adminPassword);
                if (!createResult.Succeeded)
                {
                    var errorMessages = new System.Collections.Generic.List<string>();
                    foreach (var err in createResult.Errors) errorMessages.Add(err.Description);
                    throw new InvalidOperationException(
                        $"Failed to create admin user in test DB: " +
                        string.Join("; ", errorMessages));
                }

                existingAdmin = admin;
            }

            // Ensure admin role is assigned
            if (!await userManager.IsInRoleAsync(existingAdmin, RoleNames.Admin))
            {
                await userManager.AddToRoleAsync(existingAdmin, RoleNames.Admin);
            }
        }

        private async Task<string> EnsureAdminTokenAsync()
        {
            if (_adminAccessToken is not null)
            {
                return _adminAccessToken;
            }

            // Admin seed relies on appsettings.Development.json "AdminUser" section which
            // AuthWebApplicationFactory sets UseEnvironment("Development") to load.
            // We read the configured admin credentials from the factory's IConfiguration.
            using IServiceScope scope = Factory.Services.CreateScope();

            var config = scope.ServiceProvider
                .GetRequiredService<IConfiguration>();

            string? adminEmail = config["AdminUser:Email"];
            string? adminPassword = config["AdminUser:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "AdminUser:Email / AdminUser:Password not configured in appsettings.Development.json. " +
                    "Admin token cannot be obtained for integration tests.");
            }

            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/login", new
            {
                Email = adminEmail,
                Password = adminPassword
            }, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Admin login failed ({response.StatusCode}): {body}");
            }

            var tokens = await response.Content.ReadFromJsonAsync<TokenPairDto>(JsonOptions);
            _adminAccessToken = tokens!.AccessToken;
            return _adminAccessToken;
        }

        private sealed record TokenPairDto(
            string AccessToken,
            DateTime AccessTokenExpiresAtUtc,
            string RefreshToken,
            DateTime RefreshTokenExpiresAtUtc);
    }

    /// <summary>xUnit collection definition: all Blog/Category integration tests share one factory.</summary>
    [CollectionDefinition(CollectionName)]
    public sealed class BlogApiCollection : ICollectionFixture<BlogApiFixture>
    {
        public const string CollectionName = "BlogApi";
    }
}
