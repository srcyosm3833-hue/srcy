using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.Auth
{
    /// <summary>
    /// GET /api/me — authorized, unauthenticated, invalid token.
    /// Scenario: F1-T1 #12
    /// </summary>
    [Collection(AuthApiCollection.CollectionName)]
    public sealed class MeEndpointTests
    {
        private readonly AuthApiFixture _fixture;
        private readonly HttpClient _client;

        public MeEndpointTests(AuthApiFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.CreateClient();
        }

        // ---- Valid access token returns 200 with id and email ----

        [Fact]
        public async Task GetMe_ValidAccessToken_Returns200WithIdAndEmail()
        {
            // Arrange — register + login to obtain access token
            string email = AuthApiFixture.UniqueEmail("me-valid");
            const string password = "Valid@1234";

            HttpResponseMessage reg = await _client.RegisterAsync(
                new RegisterRequest(Email: email, Password: password));
            reg.StatusCode.Should().Be(HttpStatusCode.Created);

            HttpResponseMessage login = await _client.LoginAsync(email, password);
            login.StatusCode.Should().Be(HttpStatusCode.OK);

            var tokens = await login.ReadAsAsync<TokenPairResponse>();
            tokens.Should().NotBeNull();

            // Set Bearer token on the shared client
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

            // Act
            HttpResponseMessage response = await _client.GetMeAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.TryGetProperty("id", out JsonElement idProp).Should().BeTrue("response must include id");
            idProp.GetString().Should().NotBeNullOrWhiteSpace();

            root.TryGetProperty("email", out JsonElement emailProp).Should().BeTrue("response must include email");
            emailProp.GetString().Should().Be(email);
        }

        // ---- No token returns 401 ----

        [Fact]
        public async Task GetMe_NoToken_Returns401()
        {
            // Arrange — use a fresh client via the fixture factory (uses in-memory test server)
            using HttpClient anonymousClient = _fixture.CreateClient();
            // Ensure no auth header is set
            anonymousClient.DefaultRequestHeaders.Authorization = null;

            // Act
            HttpResponseMessage response = await anonymousClient.GetMeAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                because: "the /api/me endpoint requires authentication");
        }

        // ---- Invalid / malformed token returns 401 ----

        [Fact]
        public async Task GetMe_InvalidToken_Returns401()
        {
            // Arrange — craft a clearly invalid Bearer token using the factory's test server client
            using HttpClient clientWithBadToken = _fixture.CreateClient();
            clientWithBadToken.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", "this.is.not.a.valid.jwt.token");

            // Act
            HttpResponseMessage response = await clientWithBadToken.GetMeAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                because: "a malformed JWT token must be rejected with 401");
        }

        private sealed record TokenPairResponse(
            string AccessToken,
            System.DateTime AccessTokenExpiresAtUtc,
            string RefreshToken,
            System.DateTime RefreshTokenExpiresAtUtc);
    }
}
