using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.Contact
{
    /// <summary>
    /// Integration tests for Contact endpoints (Faz 3 – Dilim B).
    ///
    /// Uses GlobalStateApiFixture (isolated DB) so that the "no contact record" 404 test
    /// is not polluted by other tests that create Contact data.
    ///
    /// Covered scenarios (sequential within one test to guarantee order):
    ///   CT-1. GET /api/contact on clean DB → 404 (no record yet)
    ///   CT-2. PUT /api/admin/contact first time (admin) → 201 + contact data
    ///   CT-3. PUT /api/admin/contact second time (different data, admin) → 200 + updated data
    ///         GET after second PUT → single record with updated data (no duplicate created)
    ///   CT-4. GET /api/contact after upsert (anonymous) → 200 + current data
    ///   CT-5. PUT /api/admin/contact as normal user → 403
    /// </summary>
    [Collection(GlobalStateApiCollection.CollectionName)]
    public sealed class ContactTests
    {
        private readonly GlobalStateApiFixture _fixture;

        public ContactTests(GlobalStateApiFixture fixture)
        {
            _fixture = fixture;
        }

        // =========================================================
        // CT-1 + CT-2 + CT-3 + CT-4: Full lifecycle test (sequential)
        //
        // These scenarios are bundled into a single test because they share global DB state.
        // Running them as separate tests in the same collection could produce ordering
        // issues (e.g., CT-1 passes only if no prior test has created a contact).
        // The single-test approach is the safest isolation strategy.
        // =========================================================

        [Fact]
        public async Task ContactLifecycle_EmptyThenUpsertTwiceThenGet_BehavesCorrectly()
        {
            using HttpClient anonClient = _fixture.CreateClient();
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();

            // --- CT-1: GET on clean DB → 404 ---
            HttpResponseMessage emptyGetResponse = await anonClient.GetContactAsync();
            emptyGetResponse.StatusCode.Should().Be(HttpStatusCode.NotFound,
                because: "no contact record has been created yet on this fresh isolated DB");

            // --- CT-2: PUT first time → 201 ---
            HttpResponseMessage firstPutResponse = await adminClient.UpsertContactAsync(
                address: "123 Main Street, Istanbul",
                email: "info@example.com",
                phone: "+90 212 000 0000",
                mapUrl: "https://maps.example.com/embed?q=istanbul");

            firstPutResponse.StatusCode.Should().Be(HttpStatusCode.Created,
                because: "first upsert must create the record and return 201");

            JsonDocument firstDoc = await firstPutResponse.ReadAsJsonDocumentAsync();
            JsonElement firstRoot = firstDoc.RootElement;

            firstRoot.TryGetProperty("id", out JsonElement firstIdProp).Should().BeTrue();
            firstRoot.GetProperty("address").GetString().Should().Be("123 Main Street, Istanbul");
            Guid firstContactId = Guid.Parse(firstIdProp.GetString()!);

            // --- CT-3: PUT second time (different data) → 200, same Id, updated values ---
            HttpResponseMessage secondPutResponse = await adminClient.UpsertContactAsync(
                address: "456 Updated Avenue, Ankara",
                email: "updated@example.com",
                phone: "+90 312 999 9999",
                mapUrl: "https://maps.example.com/embed?q=ankara");

            secondPutResponse.StatusCode.Should().Be(HttpStatusCode.OK,
                because: "second upsert on existing record must return 200 (update, not create)");

            JsonDocument secondDoc = await secondPutResponse.ReadAsJsonDocumentAsync();
            JsonElement secondRoot = secondDoc.RootElement;

            secondRoot.TryGetProperty("id", out JsonElement secondIdProp).Should().BeTrue();
            Guid secondContactId = Guid.Parse(secondIdProp.GetString()!);
            secondContactId.Should().Be(firstContactId,
                because: "upsert must update the existing record, not create a second one (same Id)");
            secondRoot.GetProperty("address").GetString().Should().Be("456 Updated Avenue, Ankara",
                because: "the update must reflect the new address value");

            // --- CT-4: GET (anonymous) after upsert → 200 + latest data ---
            HttpResponseMessage getResponse = await anonClient.GetContactAsync();
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument getDoc = await getResponse.ReadAsJsonDocumentAsync();
            JsonElement getRoot = getDoc.RootElement;

            getRoot.GetProperty("id").GetString().Should().Be(firstContactId.ToString(),
                because: "only one contact record must exist — the GET must return that same Id");
            getRoot.GetProperty("address").GetString().Should().Be("456 Updated Avenue, Ankara",
                because: "the anonymous GET must return the most recently upserted data");
        }

        // =========================================================
        // CT-5: PUT /api/admin/contact as normal user → 403
        // =========================================================

        [Fact]
        public async Task UpsertContact_NormalUser_Returns403()
        {
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("contact-ct5-user");

            HttpResponseMessage response = await userClient.UpsertContactAsync(
                address: "Should fail",
                email: "fail@example.com",
                phone: "+1 000 000 0000",
                mapUrl: "https://maps.example.com/fail");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
                because: "only admin may upsert contact information");
        }
    }
}
