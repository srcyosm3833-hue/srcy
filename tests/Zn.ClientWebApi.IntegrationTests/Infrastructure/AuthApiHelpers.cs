using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Zn.ClientWebApi.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Auth endpoint'lerine HTTP çağrısı atmayı kolaylaştıran yardımcı metotlar.
    /// Test kodundaki tekrarı azaltır ve test mantığını ön plana çıkarır.
    /// </summary>
    internal static class AuthApiHelpers
    {
        private static readonly JsonSerializerOptions JsonOpts = AuthApiFixture.JsonOptions;

        // ---- Register ----

        public static Task<HttpResponseMessage> RegisterAsync(
            this HttpClient client,
            string firstName,
            string lastName,
            string email,
            string password,
            string imageUrl = "https://example.com/avatar.png")
        {
            var payload = new
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = password,
                ImageUrl = imageUrl
            };
            return client.PostAsJsonAsync("/api/auth/register", payload, JsonOpts);
        }

        public static Task<HttpResponseMessage> RegisterAsync(this HttpClient client, RegisterRequest r) =>
            client.RegisterAsync(r.FirstName, r.LastName, r.Email, r.Password, r.ImageUrl);

        // ---- Login ----

        public static Task<HttpResponseMessage> LoginAsync(
            this HttpClient client,
            string email,
            string password)
        {
            var payload = new { Email = email, Password = password };
            return client.PostAsJsonAsync("/api/auth/login", payload, JsonOpts);
        }

        // ---- Refresh ----

        public static Task<HttpResponseMessage> RefreshAsync(this HttpClient client, string refreshToken)
        {
            var payload = new { RefreshToken = refreshToken };
            return client.PostAsJsonAsync("/api/auth/refresh", payload, JsonOpts);
        }

        // ---- Logout ----

        public static Task<HttpResponseMessage> LogoutAsync(this HttpClient client, string refreshToken)
        {
            var payload = new { RefreshToken = refreshToken };
            return client.PostAsJsonAsync("/api/auth/logout", payload, JsonOpts);
        }

        // ---- GET /api/me ----

        public static Task<HttpResponseMessage> GetMeAsync(this HttpClient client) =>
            client.GetAsync("/api/me");

        // ---- JSON deserialization helpers ----

        public static async Task<T?> ReadAsAsync<T>(this HttpResponseMessage response) =>
            await response.Content.ReadFromJsonAsync<T>(JsonOpts);

        public static async Task<JsonDocument> ReadAsJsonDocumentAsync(this HttpResponseMessage response) =>
            JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    /// <summary>Tüm zorunlu register alanlarını taşıyan DTO; testlerde kolaylık sağlar.</summary>
    internal sealed record RegisterRequest(
        string FirstName = "Test",
        string LastName = "User",
        string Email = "",
        string Password = "Test@1234",
        string ImageUrl = "https://example.com/avatar.png");
}
