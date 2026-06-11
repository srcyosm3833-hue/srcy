using System;
using System.Collections.Generic;
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
    /// Fixture for Contact and SocialMedia integration tests.
    ///
    /// These features hold GLOBAL state in the database (single contact record, shared social
    /// media list) which would pollute tests that assert on the "empty state" (Contact → 404,
    /// SocialMedia → empty list) if they shared the same DB as other tests that create such data.
    ///
    /// Solution chosen: dedicated fixture with its own isolated LocalDB database.
    /// Each test class that needs a "clean slate" can use this fixture independently.
    /// Tests within the same collection still share this single DB instance for performance;
    /// however, because the collection runs sequentially and the "empty state" assertion is done
    /// first (within a single ordered test scenario), isolation is maintained.
    /// </summary>
    public sealed class GlobalStateApiFixture : IAsyncLifetime
    {
        public AuthWebApplicationFactory Factory { get; } = new();

        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private string? _adminAccessToken;

        public HttpClient CreateClient() =>
            Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        public async Task<HttpClient> CreateAdminClientAsync()
        {
            string token = await EnsureAdminTokenAsync();
            HttpClient client = CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<(HttpClient Client, string UserId, string AccessToken)> CreateUserClientAsync(
            string prefix = "user")
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

            if (!reg.IsSuccessStatusCode)
            {
                string regBody = await reg.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"User registration failed ({reg.StatusCode}): {regBody}");
            }

            string regBodyStr = await reg.Content.ReadAsStringAsync();
            var regDoc = JsonDocument.Parse(regBodyStr);
            string userId = regDoc.RootElement.GetProperty("id").GetString()!;

            HttpResponseMessage login = await anon.PostAsJsonAsync("/api/auth/login", new
            {
                Email = email,
                Password = password
            }, JsonOptions);

            if (!login.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"User login failed ({login.StatusCode})");
            }

            var tokens = await login.Content.ReadFromJsonAsync<TokenPairDto>(JsonOptions);
            string accessToken = tokens!.AccessToken;

            HttpClient userClient = CreateClient();
            userClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            return (userClient, userId, accessToken);
        }

        public async Task InitializeAsync()
        {
            await Factory.InitializeAsync();
            await EnsureAdminUserSeededAsync();
        }

        public async Task DisposeAsync() => await Factory.DisposeAsync();

        public static string UniqueEmail(string prefix = "testuser") =>
            $"{prefix}-{Guid.NewGuid():N}@globalstate.test";

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
                    "AdminUser:Email / AdminUser:Password not configured.");
            }

            var roleManager = provider.GetRequiredService<RoleManager<Role>>();
            var userManager = provider.GetRequiredService<UserManager<User>>();

            if (!await roleManager.RoleExistsAsync(RoleNames.Admin))
            {
                await roleManager.CreateAsync(new Role { Name = RoleNames.Admin });
            }

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
                    var errors = new List<string>();
                    foreach (var err in createResult.Errors) errors.Add(err.Description);
                    throw new InvalidOperationException(
                        $"Failed to create admin user in test DB: {string.Join("; ", errors)}");
                }

                existingAdmin = admin;
            }

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

            using IServiceScope scope = Factory.Services.CreateScope();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            string? adminEmail = config["AdminUser:Email"];
            string? adminPassword = config["AdminUser:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "AdminUser:Email / AdminUser:Password not configured.");
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

    /// <summary>
    /// xUnit collection definition for Contact and SocialMedia tests.
    /// Uses a dedicated fixture with its own isolated DB to prevent "empty state" pollution.
    /// </summary>
    [CollectionDefinition(CollectionName)]
    public sealed class GlobalStateApiCollection : ICollectionFixture<GlobalStateApiFixture>
    {
        public const string CollectionName = "GlobalStateApi";
    }
}
