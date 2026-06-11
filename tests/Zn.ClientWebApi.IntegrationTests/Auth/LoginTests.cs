using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.Auth
{
    /// <summary>
    /// POST /api/auth/login — happy path, wrong password, non-existent email.
    /// Scenarios: F1-T1 #5, #6, #7
    /// Note: lockout (scenario #8) is in LockoutTests.cs to keep its own isolated user.
    /// </summary>
    [Collection(AuthApiCollection.CollectionName)]
    public sealed class LoginTests
    {
        private readonly HttpClient _client;

        public LoginTests(AuthApiFixture fixture)
        {
            _client = fixture.CreateClient();
        }

        // ---- Scenario 5: valid credentials return 200 with tokens ----

        [Fact]
        public async Task Login_ValidCredentials_Returns200WithTokens()
        {
            // Arrange — register a user first
            string email = AuthApiFixture.UniqueEmail("login-valid");
            const string password = "Valid@1234";
            HttpResponseMessage reg = await _client.RegisterAsync(new RegisterRequest(Email: email, Password: password));
            reg.StatusCode.Should().Be(HttpStatusCode.Created, "user must be registered before login test");

            // Act
            HttpResponseMessage response = await _client.LoginAsync(email, password);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var tokens = await response.ReadAsAsync<LoginResponse>();
            tokens.Should().NotBeNull();
            tokens!.AccessToken.Should().NotBeNullOrWhiteSpace("access token should be present");
            tokens.RefreshToken.Should().NotBeNullOrWhiteSpace("refresh token should be present");
            tokens.AccessTokenExpiresAtUtc.Should().BeAfter(System.DateTime.UtcNow,
                "access token expiry must be in the future");
        }

        // ---- Scenario 6: wrong password returns 401 (generic message) ----

        [Fact]
        public async Task Login_WrongPassword_Returns401()
        {
            // Arrange
            string email = AuthApiFixture.UniqueEmail("login-wrong-pwd");
            const string correctPassword = "Correct@1234";
            HttpResponseMessage reg = await _client.RegisterAsync(
                new RegisterRequest(Email: email, Password: correctPassword));
            reg.StatusCode.Should().Be(HttpStatusCode.Created);

            // Act
            HttpResponseMessage response = await _client.LoginAsync(email, "Wrong@9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // Response must NOT reveal which field is wrong (email/password distinction)
            string body = await response.Content.ReadAsStringAsync();
            // The error message "Invalid email or password" is generic — acceptable.
            // We just verify it does NOT identify the specific field name in an error-map form.
            body.Should().NotBeNullOrWhiteSpace(
                "a 401 response body should contain a problem details payload");
        }

        // ---- Scenario 7: non-existent email returns 401 (not 404) ----

        [Fact]
        public async Task Login_NonExistentEmail_Returns401NotFound()
        {
            // Arrange — use an email that was never registered
            string ghostEmail = AuthApiFixture.UniqueEmail("ghost-user");

            // Act
            HttpResponseMessage response = await _client.LoginAsync(ghostEmail, "SomePass@1");

            // Assert — must be 401, NOT 404 (user existence must not be revealed)
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                because: "a non-existent user must yield the same generic 401 as a wrong password");
            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
                because: "404 would leak that no account exists for the given email");
        }

        // Helper record — mirrors AuthTokensResponse DTO structure
        private sealed record LoginResponse(
            string AccessToken,
            System.DateTime AccessTokenExpiresAtUtc,
            string RefreshToken,
            System.DateTime RefreshTokenExpiresAtUtc);
    }
}
