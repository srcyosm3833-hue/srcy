using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.Auth
{
    /// <summary>
    /// POST /api/auth/refresh — rotation, replay detection (chain revocation).
    /// Scenarios: F1-T1 #9, #10
    /// </summary>
    [Collection(AuthApiCollection.CollectionName)]
    public sealed class RefreshTokenTests
    {
        private readonly HttpClient _client;

        public RefreshTokenTests(AuthApiFixture fixture)
        {
            _client = fixture.CreateClient();
        }

        // ---- Scenario 9: valid refresh token returns new access + refresh tokens ----

        [Fact]
        public async Task Refresh_ValidToken_Returns200WithNewTokenPair()
        {
            // Arrange — register + login to obtain initial tokens
            (string _, TokenPair initial) = await RegisterAndLoginAsync("refresh-valid");

            // Act
            HttpResponseMessage response = await _client.RefreshAsync(initial.RefreshToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var newTokens = await response.ReadAsAsync<TokenPairResponse>();
            newTokens.Should().NotBeNull();
            newTokens!.AccessToken.Should().NotBeNullOrWhiteSpace();
            newTokens.RefreshToken.Should().NotBeNullOrWhiteSpace();

            // New refresh token must be DIFFERENT from the original (rotation)
            newTokens.RefreshToken.Should().NotBe(initial.RefreshToken,
                because: "token rotation must issue a brand-new refresh token");

            // New access token must expire in the future
            newTokens.AccessTokenExpiresAtUtc.Should().BeAfter(DateTime.UtcNow);
        }

        // ---- Scenario 10: replay attack → 401 + chain revocation ----

        [Fact]
        public async Task Refresh_RevokedToken_Returns401AndRevokesChain()
        {
            // Arrange
            (string _, TokenPair initial) = await RegisterAndLoginAsync("refresh-replay");

            // First use of the refresh token — succeeds and rotates
            HttpResponseMessage firstRefresh = await _client.RefreshAsync(initial.RefreshToken);
            firstRefresh.StatusCode.Should().Be(HttpStatusCode.OK,
                because: "the first refresh with a valid token should succeed");

            var rotated = await firstRefresh.ReadAsAsync<TokenPairResponse>();
            rotated.Should().NotBeNull();
            string newRefreshToken = rotated!.RefreshToken;

            // Act — replay: use the OLD (now revoked) refresh token again
            HttpResponseMessage replayResponse = await _client.RefreshAsync(initial.RefreshToken);

            // Assert — replay must return 401
            replayResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                because: "replaying a revoked refresh token must return 401");

            // Chain revocation: the newly rotated refresh token must also be invalidated
            // (the handler calls RevokeAllActiveForUserAsync when replay is detected)
            HttpResponseMessage chainRevokedResponse = await _client.RefreshAsync(newRefreshToken);
            chainRevokedResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                because: "chain revocation must invalidate all active tokens for the user, " +
                         "including the one issued after the rotated token");
        }

        // ---- Helper: register + login, return email + token pair ----

        private async Task<(string Email, TokenPair Tokens)> RegisterAndLoginAsync(string prefix)
        {
            string email = AuthApiFixture.UniqueEmail(prefix);
            const string password = "Valid@1234";

            HttpResponseMessage reg = await _client.RegisterAsync(
                new RegisterRequest(Email: email, Password: password));
            reg.StatusCode.Should().Be(HttpStatusCode.Created);

            HttpResponseMessage login = await _client.LoginAsync(email, password);
            login.StatusCode.Should().Be(HttpStatusCode.OK);

            var tokens = await login.ReadAsAsync<TokenPairResponse>();
            tokens.Should().NotBeNull();

            return (email, new TokenPair(tokens!.AccessToken, tokens.RefreshToken));
        }

        private sealed record TokenPair(string AccessToken, string RefreshToken);

        private sealed record TokenPairResponse(
            string AccessToken,
            DateTime AccessTokenExpiresAtUtc,
            string RefreshToken,
            DateTime RefreshTokenExpiresAtUtc);
    }
}
