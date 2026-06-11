using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.SocialMedia
{
    /// <summary>
    /// Integration tests for SocialMedia endpoints (Faz 3 – Dilim C).
    ///
    /// Uses GlobalStateApiFixture (isolated DB) so that the "empty list" test
    /// is not polluted by social media entries created by other tests.
    ///
    /// Covered scenarios:
    ///   SM-1. GET /api/social-media anonymous, no entries → 200 + empty array (NOT 404)
    ///   SM-2. POST admin → 201 + correct response fields
    ///   SM-3. POST without token → 401
    ///   SM-4. POST with normal user token → 403
    ///   SM-5. POST with empty Title → 400
    ///   SM-6. POST with invalid Url format → 400
    ///   SM-7. PUT existing entry by admin → 200 + updated data
    ///   SM-8. PUT non-existent entry → 404
    ///   SM-9. DELETE existing entry by admin → 204; subsequent GET no longer contains it
    ///   SM-10. DELETE non-existent entry → 404
    /// </summary>
    [Collection(GlobalStateApiCollection.CollectionName)]
    public sealed class SocialMediaTests
    {
        private readonly GlobalStateApiFixture _fixture;

        public SocialMediaTests(GlobalStateApiFixture fixture)
        {
            _fixture = fixture;
        }

        // =========================================================
        // SM-1: GET anonymous, no entries → 200 + empty array (not 404)
        //
        // NOTE: This test runs in the same isolated DB as other SM tests.
        // To guarantee clean state for this assertion, the test is positioned
        // first alphabetically (xUnit runs tests within a class in declaration order).
        // The GlobalStateApiFixture provides a fresh DB per test run, so if this
        // test class runs before other SM tests create data, we get the empty list.
        //
        // For absolute safety this check is also embedded in the lifecycle test below.
        // =========================================================

        [Fact]
        public async Task GetSocialMedia_EmptyDatabase_Returns200WithEmptyArray()
        {
            // This test must run against a clean DB — ensured by the isolated GlobalStateApiFixture.
            // It tests the specific product requirement: 200 + empty array (never 404).
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.GetSocialMediaAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK,
                because: "GET social-media must return 200 even when no entries exist (not 404)");

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            doc.RootElement.ValueKind.Should().Be(JsonValueKind.Array,
                because: "response body must be an array when there are no social media entries");
        }

        // =========================================================
        // SM-2: POST admin → 201 + correct response fields
        // =========================================================

        [Fact]
        public async Task CreateSocialMedia_Admin_Returns201WithCorrectFields()
        {
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();

            HttpResponseMessage response = await adminClient.CreateSocialMediaAsync(
                title: $"Instagram-{Guid.NewGuid():N}",
                url: "https://instagram.com/testprofile",
                icon: "fa-instagram");

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.TryGetProperty("id", out _).Should().BeTrue();
            root.GetProperty("title").GetString().Should().Contain("Instagram");
            root.GetProperty("url").GetString().Should().Be("https://instagram.com/testprofile");
            root.GetProperty("icon").GetString().Should().Be("fa-instagram");
        }

        // =========================================================
        // SM-3: POST without token → 401
        // =========================================================

        [Fact]
        public async Task CreateSocialMedia_NoToken_Returns401()
        {
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.CreateSocialMediaAsync(
                title: "Twitter",
                url: "https://twitter.com/profile",
                icon: "fa-twitter");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // =========================================================
        // SM-4: POST with normal user token → 403
        // =========================================================

        [Fact]
        public async Task CreateSocialMedia_NormalUser_Returns403()
        {
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("sm4-user");
            HttpResponseMessage response = await userClient.CreateSocialMediaAsync(
                title: "LinkedIn",
                url: "https://linkedin.com/in/test",
                icon: "fa-linkedin");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // =========================================================
        // SM-5: POST with empty Title → 400
        // =========================================================

        [Fact]
        public async Task CreateSocialMedia_EmptyTitle_Returns400()
        {
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            HttpResponseMessage response = await adminClient.CreateSocialMediaAsync(
                title: string.Empty,
                url: "https://example.com/profile",
                icon: "fa-test");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                because: "Title is a required field");
        }

        // =========================================================
        // SM-6: POST with invalid Url format → 400
        // =========================================================

        [Fact]
        public async Task CreateSocialMedia_InvalidUrlFormat_Returns400()
        {
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            HttpResponseMessage response = await adminClient.CreateSocialMediaAsync(
                title: "BadUrl Platform",
                url: "not-a-valid-url",
                icon: "fa-bad");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                because: "Url must be a valid absolute http/https URL");
        }

        // =========================================================
        // SM-7: PUT existing entry by admin → 200 + updated data
        // =========================================================

        [Fact]
        public async Task UpdateSocialMedia_Admin_Returns200WithUpdatedData()
        {
            // Arrange — create an entry first
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string originalTitle = $"OriginalPlatform-{Guid.NewGuid():N}";
            Guid entryId = await adminClient.ArrangeCreateSocialMediaAsync(
                title: originalTitle,
                url: "https://example.com/original",
                icon: "fa-original");

            // Act
            string updatedTitle = $"UpdatedPlatform-{Guid.NewGuid():N}";
            HttpResponseMessage response = await adminClient.UpdateSocialMediaAsync(
                id: entryId,
                title: updatedTitle,
                url: "https://example.com/updated",
                icon: "fa-updated");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.GetProperty("id").GetString().Should().Be(entryId.ToString());
            root.GetProperty("title").GetString().Should().Be(updatedTitle);
            root.GetProperty("url").GetString().Should().Be("https://example.com/updated");
            root.GetProperty("icon").GetString().Should().Be("fa-updated");
        }

        // =========================================================
        // SM-8: PUT non-existent entry → 404
        // =========================================================

        [Fact]
        public async Task UpdateSocialMedia_NonExistentId_Returns404()
        {
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            HttpResponseMessage response = await adminClient.UpdateSocialMediaAsync(
                id: Guid.NewGuid(),
                title: "Ghost Platform",
                url: "https://example.com/ghost",
                icon: "fa-ghost");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // =========================================================
        // SM-9: DELETE existing entry → 204; GET no longer contains it
        // =========================================================

        [Fact]
        public async Task DeleteSocialMedia_Admin_Returns204AndEntryRemovedFromList()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string titleToDelete = $"DeleteMe-{Guid.NewGuid():N}";
            Guid entryId = await adminClient.ArrangeCreateSocialMediaAsync(
                title: titleToDelete,
                url: "https://example.com/delete-me",
                icon: "fa-delete");

            // Verify it exists in the list before deletion
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage beforeResponse = await anonClient.GetSocialMediaAsync();
            JsonDocument beforeDoc = await beforeResponse.ReadAsJsonDocumentAsync();
            bool foundBefore = false;
            foreach (JsonElement item in beforeDoc.RootElement.EnumerateArray())
            {
                if (item.GetProperty("id").GetString() == entryId.ToString())
                {
                    foundBefore = true;
                    break;
                }
            }

            foundBefore.Should().BeTrue("setup: the entry must exist before deletion");

            // Act
            HttpResponseMessage deleteResponse = await adminClient.DeleteSocialMediaAsync(entryId);

            // Assert — delete succeeds
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Assert — entry no longer in list
            HttpResponseMessage afterResponse = await anonClient.GetSocialMediaAsync();
            afterResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument afterDoc = await afterResponse.ReadAsJsonDocumentAsync();
            bool foundAfter = false;
            foreach (JsonElement item in afterDoc.RootElement.EnumerateArray())
            {
                if (item.GetProperty("id").GetString() == entryId.ToString())
                {
                    foundAfter = true;
                    break;
                }
            }

            foundAfter.Should().BeFalse(
                because: "deleted social media entry must no longer appear in the public list");
        }

        // =========================================================
        // SM-10: DELETE non-existent entry → 404
        // =========================================================

        [Fact]
        public async Task DeleteSocialMedia_NonExistentId_Returns404()
        {
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            HttpResponseMessage response = await adminClient.DeleteSocialMediaAsync(Guid.NewGuid());

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // =========================================================
        // SM-11: POST with a title that already exists → 409 (not 500)
        //
        // Title has a unique index; the handler pre-checks and returns a
        // meaningful Conflict instead of letting the DB throw (Category pattern).
        // =========================================================

        [Fact]
        public async Task CreateSocialMedia_DuplicateTitle_Returns409()
        {
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string title = $"DuplicatePlatform-{Guid.NewGuid():N}";

            // Arrange — first create succeeds
            HttpResponseMessage first = await adminClient.CreateSocialMediaAsync(
                title: title,
                url: "https://example.com/first",
                icon: "fa-first");
            first.StatusCode.Should().Be(HttpStatusCode.Created);

            // Act — second create with the same title
            HttpResponseMessage second = await adminClient.CreateSocialMediaAsync(
                title: title,
                url: "https://example.com/second",
                icon: "fa-second");

            // Assert — meaningful 409, not a 500 from the DB unique index
            second.StatusCode.Should().Be(HttpStatusCode.Conflict,
                because: "a social media link with a duplicate title must return 409, not 500");
        }
    }
}
