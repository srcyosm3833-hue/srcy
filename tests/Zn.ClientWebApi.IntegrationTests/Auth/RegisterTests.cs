using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.Auth
{
    /// <summary>
    /// POST /api/auth/register — happy path, validation failures, duplicate email.
    /// Scenarios: F1-T1 #1, #2, #3, #4
    /// </summary>
    [Collection(AuthApiCollection.CollectionName)]
    public sealed class RegisterTests
    {
        private readonly HttpClient _client;

        public RegisterTests(AuthApiFixture fixture)
        {
            _client = fixture.CreateClient();
        }

        // ---- Scenario 1: valid registration returns 201 with id + email, no password ----

        [Fact]
        public async Task Register_ValidRequest_Returns201WithIdAndEmail()
        {
            // Arrange
            string email = AuthApiFixture.UniqueEmail("register-valid");
            var request = new RegisterRequest(Email: email, Password: "Valid@1234");

            // Act
            HttpResponseMessage response = await _client.RegisterAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.TryGetProperty("id", out JsonElement idProp).Should().BeTrue("response should include id");
            idProp.GetString().Should().NotBeNullOrWhiteSpace();

            root.TryGetProperty("email", out JsonElement emailProp).Should().BeTrue("response should include email");
            emailProp.GetString().Should().Be(email);

            // Password/hash must NOT be present anywhere in the response
            string rawJson = doc.RootElement.GetRawText().ToLowerInvariant();
            rawJson.Should().NotContain("password",
                "sensitive fields must never be returned in the registration response");
            rawJson.Should().NotContain("hash",
                "sensitive fields must never be returned in the registration response");
        }

        // ---- Scenario 2: weak password returns 400 with field-level validation error ----

        [Theory]
        [InlineData("short", "Password is too short")]          // < 8 chars
        [InlineData("alllowercase1", "No uppercase")]           // no uppercase
        [InlineData("ALLUPPERCASE1", "No lowercase")]           // no lowercase
        [InlineData("NoDigitHere!", "No digit")]                // no digit
        [InlineData("", "Empty password")]                      // empty
        public async Task Register_WeakPassword_Returns400WithPasswordError(string weakPassword, string reason)
        {
            // Arrange
            string email = AuthApiFixture.UniqueEmail("register-weak-pwd");
            var request = new RegisterRequest(Email: email, Password: weakPassword);

            // Act
            HttpResponseMessage response = await _client.RegisterAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                because: $"weak password '{weakPassword}' should be rejected ({reason})");

            string body = await response.Content.ReadAsStringAsync();
            body.Should().NotBeNullOrWhiteSpace();
        }

        // ---- Scenario 3: duplicate email returns 409 ----

        [Fact]
        public async Task Register_DuplicateEmail_Returns409()
        {
            // Arrange
            string email = AuthApiFixture.UniqueEmail("register-dup");
            var request = new RegisterRequest(Email: email, Password: "Valid@1234");

            // First registration must succeed
            HttpResponseMessage first = await _client.RegisterAsync(request);
            first.StatusCode.Should().Be(HttpStatusCode.Created, "first registration should succeed");

            // Act — second registration with the same email
            HttpResponseMessage response = await _client.RegisterAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        // ---- Scenario 4: missing required fields return 400 ----

        [Fact]
        public async Task Register_EmptyFirstName_Returns400()
        {
            // Arrange
            string email = AuthApiFixture.UniqueEmail("register-no-fname");
            var request = new RegisterRequest(FirstName: "", Email: email);

            // Act
            HttpResponseMessage response = await _client.RegisterAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            string body = await response.Content.ReadAsStringAsync();
            // FluentValidation pipeline (Wolverine middleware) raises ValidationException → 400
            body.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Register_EmptyLastName_Returns400()
        {
            // Arrange
            string email = AuthApiFixture.UniqueEmail("register-no-lname");
            var request = new RegisterRequest(LastName: "", Email: email);

            // Act
            HttpResponseMessage response = await _client.RegisterAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_EmptyEmail_Returns400()
        {
            // Arrange — email is intentionally empty / missing
            var request = new RegisterRequest(Email: "");

            // Act
            HttpResponseMessage response = await _client.RegisterAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_InvalidEmailFormat_Returns400()
        {
            // Arrange
            var request = new RegisterRequest(Email: "not-an-email");

            // Act
            HttpResponseMessage response = await _client.RegisterAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
