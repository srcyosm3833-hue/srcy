using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.Auth
{
    /// <summary>
    /// POST /api/auth/logout — revoke token; post-logout refresh returns 401.
    /// Scenario: F1-T1 #11
    /// </summary>
    [Collection(AuthApiCollection.CollectionName)]
    public sealed class LogoutTests
    {
        private readonly HttpClient _client;

        public LogoutTests(AuthApiFixture fixture)
        {
            _client = fixture.CreateClient();
        }

        [Fact]
        public async Task Logout_ValidToken_Returns204AndInvalidatesRefreshToken()
        {
            // Arrange — register + login
            string email = AuthApiFixture.UniqueEmail("logout-user");
            const string password = "Valid@1234";

            HttpResponseMessage reg = await _client.RegisterAsync(
                new RegisterRequest(Email: email, Password: password));
            reg.StatusCode.Should().Be(HttpStatusCode.Created);

            HttpResponseMessage login = await _client.LoginAsync(email, password);
            login.StatusCode.Should().Be(HttpStatusCode.OK);

            var tokens = await login.ReadAsAsync<TokenPairResponse>();
            tokens.Should().NotBeNull();
            string refreshToken = tokens!.RefreshToken;

            // Act — logout
            HttpResponseMessage logoutResponse = await _client.LogoutAsync(refreshToken);

            // Assert — 204 No Content
            logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
                because: "a successful logout must return 204");

            // Post-logout: the same refresh token must no longer be usable
            HttpResponseMessage refreshAfterLogout = await _client.RefreshAsync(refreshToken);
            refreshAfterLogout.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                because: "the refresh token was revoked during logout; reusing it must return 401");
        }

        [Fact]
        public async Task Logout_UnknownToken_Returns204Idempotent()
        {
            // Logout with a token that never existed — must still return 204 (idempotent)
            HttpResponseMessage response = await _client.LogoutAsync("nonexistent-token-value-xyz");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent,
                because: "logout is idempotent; unknown tokens should not cause errors");
        }

        private sealed record TokenPairResponse(
            string AccessToken,
            System.DateTime AccessTokenExpiresAtUtc,
            string RefreshToken,
            System.DateTime RefreshTokenExpiresAtUtc);
    }
}
