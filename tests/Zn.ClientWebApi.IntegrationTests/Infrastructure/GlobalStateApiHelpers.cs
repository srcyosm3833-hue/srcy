using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Zn.ClientWebApi.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Extension methods for Contact and SocialMedia HTTP calls used in GlobalStateApi tests.
    /// Mirrors the BlogApiHelpers pattern for consistency.
    /// </summary>
    internal static class GlobalStateApiHelpers
    {
        private static readonly JsonSerializerOptions JsonOpts = GlobalStateApiFixture.JsonOptions;

        // ---- Contact (public read) ----

        public static Task<HttpResponseMessage> GetContactAsync(this HttpClient client) =>
            client.GetAsync("/api/contact");

        // ---- Contact (admin write) ----

        public static Task<HttpResponseMessage> UpsertContactAsync(
            this HttpClient client,
            string address,
            string email,
            string phone,
            string mapUrl) =>
            client.PutAsJsonAsync("/api/admin/contact", new
            {
                Address = address,
                Email = email,
                Phone = phone,
                MapUrl = mapUrl
            }, JsonOpts);

        // ---- SocialMedia (public read) ----

        public static Task<HttpResponseMessage> GetSocialMediaAsync(this HttpClient client) =>
            client.GetAsync("/api/social-media");

        // ---- SocialMedia (admin write) ----

        public static Task<HttpResponseMessage> CreateSocialMediaAsync(
            this HttpClient client, string title, string url, string icon) =>
            client.PostAsJsonAsync("/api/admin/social-media", new
            {
                Title = title,
                Url = url,
                Icon = icon
            }, JsonOpts);

        public static Task<HttpResponseMessage> UpdateSocialMediaAsync(
            this HttpClient client, Guid id, string title, string url, string icon) =>
            client.PutAsJsonAsync($"/api/admin/social-media/{id}", new
            {
                Title = title,
                Url = url,
                Icon = icon
            }, JsonOpts);

        public static Task<HttpResponseMessage> DeleteSocialMediaAsync(
            this HttpClient client, Guid id) =>
            client.DeleteAsync($"/api/admin/social-media/{id}");

        /// <summary>
        /// Creates a social media entry and returns its Id. Throws on failure.
        /// </summary>
        public static async Task<Guid> ArrangeCreateSocialMediaAsync(
            this HttpClient adminClient,
            string title,
            string url = "https://example.com/profile",
            string icon = "fa-test")
        {
            HttpResponseMessage response = await adminClient.CreateSocialMediaAsync(title, url, icon);
            string body = await response.Content.ReadAsStringAsync();
            if ((int)response.StatusCode != 201)
            {
                throw new InvalidOperationException(
                    $"Social media creation failed ({response.StatusCode}): {body}");
            }

            JsonDocument doc = JsonDocument.Parse(body);
            string idStr = doc.RootElement.GetProperty("id").GetString()!;
            return Guid.Parse(idStr);
        }
    }
}
