using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.Audit
{
    /// <summary>
    /// Integration tests for the Search Audit Log feature.
    ///
    /// Endpoint under test: GET /api/blogs/search?q=...  (public, writes SearchLog as side-effect)
    /// Admin log endpoint:  GET /api/admin/search-logs   (Admin only)
    ///
    /// Test environment: Audit:IpHashSalt = "zn-dev-ip-salt"
    ///
    /// Covered scenarios:
    ///   SL-01. Authenticated user search → SearchLog created with UserId + UserFullName + IpHash + Term
    ///   SL-02. Anonymous search with X-Forwarded-For → log created with null UserId, null UserFullName, IpHash dolu
    ///   SL-03. Zero-result search → log record is still created
    ///   SL-04. Invalid/empty q → 400 and NO log record written
    ///   SL-05. GET /api/admin/search-logs with Admin token → 200 + PagedResult shape
    ///   SL-06. GET /api/admin/search-logs anonymous → 401
    ///   SL-07. GET /api/admin/search-logs with Manager token → 403
    ///   SL-08. GET /api/admin/search-logs term filter (case-insensitive) → only matching logs
    /// </summary>
    [Collection(BlogApiCollection.CollectionName)]
    public sealed class SearchLogTests
    {
        private const string SampleIp = "203.0.113.77";

        private readonly BlogApiFixture _fixture;

        public SearchLogTests(BlogApiFixture fixture)
        {
            _fixture = fixture;
        }

        // =========================================================
        // SL-01: Authenticated user search → log with UserId + UserFullName + IpHash + Term
        // =========================================================

        [Fact]
        public async Task SearchBlogs_AuthenticatedUser_LogRecordContainsUserIdAndFullName()
        {
            // Arrange — create a user to have a known full name
            (HttpClient userClient, string userId, _) =
                await _fixture.CreateUserClientAsync("sl01-user");

            // Use a unique term to find this specific log entry later
            string uniqueTerm = $"SL01term{Guid.NewGuid():N}";

            // Send the search request with a known IP header so we can verify IpHash
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/blogs/search?q={Uri.EscapeDataString(uniqueTerm)}");
            request.Headers.Add("X-Forwarded-For", SampleIp);

            // Reuse the user client's auth header but set IP via the raw request
            // (the user client's DefaultRequestHeaders are already set)
            foreach (var header in userClient.DefaultRequestHeaders)
            {
                if (!request.Headers.Contains(header.Key))
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            HttpResponseMessage searchResp = await userClient.SendAsync(request);
            userClient.Dispose();

            searchResp.StatusCode.Should().Be(HttpStatusCode.OK,
                because: "search must succeed even with no results to create the log");

            // Act — retrieve logs from admin endpoint and find the entry for our unique term
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            HttpResponseMessage logResp = await adminClient.GetAdminSearchLogsAsync(
                page: 1, pageSize: 50, term: uniqueTerm);

            logResp.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument logDoc = JsonDocument.Parse(await logResp.Content.ReadAsStringAsync());
            JsonElement items = logDoc.RootElement.GetProperty("items");

            // Assert — find the log entry matching our term
            JsonElement? logEntry = FindLogByTerm(items, uniqueTerm);
            logEntry.Should().NotBeNull(
                because: $"a search log for term '{uniqueTerm}' must have been created");

            JsonElement entry = logEntry!.Value;
            entry.GetProperty("term").GetString().Should().Be(uniqueTerm,
                because: "log term must match the search query exactly");
            entry.GetProperty("userId").GetString().Should().Be(userId,
                because: "authenticated user's id must be captured in the log");
            entry.GetProperty("userFullName").GetString().Should().NotBeNullOrWhiteSpace(
                because: "authenticated user's full name snapshot must be captured");

            // IpHash must be non-null (we sent X-Forwarded-For)
            entry.TryGetProperty("ipHash", out JsonElement ipHashProp).Should().BeTrue();
            ipHashProp.ValueKind.Should().NotBe(JsonValueKind.Null,
                because: "IpHash must be set when X-Forwarded-For was provided");
            ipHashProp.GetString().Should().NotBe(SampleIp,
                because: "IpHash must be the hash, not the raw IP");
        }

        // =========================================================
        // SL-02: Anonymous search with X-Forwarded-For → null UserId, null UserFullName, IpHash dolu
        // =========================================================

        [Fact]
        public async Task SearchBlogs_AnonymousWithIp_LogRecordHasNullUserFieldsButPopulatedIpHash()
        {
            // Arrange
            string uniqueTerm = $"SL02anon{Guid.NewGuid():N}";
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage searchResp = await anonClient.SearchBlogsWithIpAsync(
                uniqueTerm, SampleIp);
            searchResp.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            HttpResponseMessage logResp = await adminClient.GetAdminSearchLogsAsync(
                page: 1, pageSize: 50, term: uniqueTerm);
            logResp.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument logDoc = JsonDocument.Parse(await logResp.Content.ReadAsStringAsync());
            JsonElement items = logDoc.RootElement.GetProperty("items");

            // Assert
            JsonElement? logEntry = FindLogByTerm(items, uniqueTerm);
            logEntry.Should().NotBeNull(
                because: "anonymous search must still create a log entry");

            JsonElement entry = logEntry!.Value;

            // UserId must be null for anonymous searches
            entry.TryGetProperty("userId", out JsonElement userIdProp).Should().BeTrue();
            userIdProp.ValueKind.Should().Be(JsonValueKind.Null,
                because: "anonymous searches have no UserId");

            // UserFullName must be null for anonymous searches
            entry.TryGetProperty("userFullName", out JsonElement userFullNameProp).Should().BeTrue();
            userFullNameProp.ValueKind.Should().Be(JsonValueKind.Null,
                because: "anonymous searches have no UserFullName");

            // IpHash must be non-null when X-Forwarded-For was provided
            entry.TryGetProperty("ipHash", out JsonElement ipHashProp).Should().BeTrue();
            ipHashProp.ValueKind.Should().NotBe(JsonValueKind.Null,
                because: "IpHash must be populated even for anonymous searches when IP is known");
            ipHashProp.GetString().Should().NotBe(SampleIp,
                because: "stored value must be the hash, not the raw IP");
        }

        // =========================================================
        // SL-03: Zero-result search → log record is still created
        // =========================================================

        [Fact]
        public async Task SearchBlogs_ZeroResults_LogRecordStillCreated()
        {
            // Arrange — use a term that will definitely find nothing
            string noMatchTerm = $"ZZZNOMATCH{Guid.NewGuid():N}ZZZNOMATCH";
            using HttpClient anonClient = _fixture.CreateClient();

            HttpResponseMessage searchResp = await anonClient.SearchBlogsAsync(noMatchTerm);
            searchResp.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument searchDoc = JsonDocument.Parse(await searchResp.Content.ReadAsStringAsync());
            searchDoc.RootElement.GetProperty("totalCount").GetInt32().Should().Be(0,
                because: "the term must return zero results for this test to be meaningful");

            // Act — check if log was created despite zero results
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            HttpResponseMessage logResp = await adminClient.GetAdminSearchLogsAsync(
                page: 1, pageSize: 50, term: noMatchTerm);
            logResp.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument logDoc = JsonDocument.Parse(await logResp.Content.ReadAsStringAsync());
            JsonElement items = logDoc.RootElement.GetProperty("items");

            // Assert
            JsonElement? logEntry = FindLogByTerm(items, noMatchTerm);
            logEntry.Should().NotBeNull(
                because: "a search log must be written even when the search returns zero results");
        }

        // =========================================================
        // SL-04: Invalid/empty q → 400 and NO log record written
        // =========================================================

        [Fact]
        public async Task SearchBlogs_EmptyQuery_Returns400AndNoLogRecordCreated()
        {
            // Arrange — capture log count before the failed request
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            HttpResponseMessage countBefore = await adminClient.GetAdminSearchLogsAsync(page: 1, pageSize: 1);
            countBefore.StatusCode.Should().Be(HttpStatusCode.OK);
            int totalBefore = JsonDocument.Parse(await countBefore.Content.ReadAsStringAsync())
                .RootElement.GetProperty("totalCount").GetInt32();

            // Act — send invalid request (empty q)
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage badResp = await anonClient.GetAsync("/api/blogs/search?q=");

            // Assert — request must be rejected
            badResp.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                because: "empty search query must be rejected by FluentValidation before reaching the handler");

            // Assert — log count must not have increased (validator blocks before handler runs)
            HttpResponseMessage countAfter = await adminClient.GetAdminSearchLogsAsync(page: 1, pageSize: 1);
            countAfter.StatusCode.Should().Be(HttpStatusCode.OK);
            int totalAfter = JsonDocument.Parse(await countAfter.Content.ReadAsStringAsync())
                .RootElement.GetProperty("totalCount").GetInt32();

            totalAfter.Should().Be(totalBefore,
                because: "no log record must be written when the request fails validation before reaching the handler");
        }

        // =========================================================
        // SL-05: GET /api/admin/search-logs with Admin token → 200 + PagedResult shape
        // =========================================================

        [Fact]
        public async Task GetSearchLogs_AdminToken_Returns200WithPagedShape()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();

            // Act
            HttpResponseMessage response = await adminClient.GetAdminSearchLogsAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                because: "Admin token must be able to access search logs");

            JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            JsonElement root = doc.RootElement;

            root.TryGetProperty("items", out _).Should().BeTrue("paged result must have items");
            root.TryGetProperty("totalCount", out _).Should().BeTrue("paged result must have totalCount");
            root.TryGetProperty("page", out JsonElement pageProp).Should().BeTrue("paged result must have page");
            root.TryGetProperty("pageSize", out _).Should().BeTrue("paged result must have pageSize");
            root.TryGetProperty("totalPages", out _).Should().BeTrue("paged result must have totalPages");

            pageProp.GetInt32().Should().Be(1, because: "default page is 1");
        }

        // =========================================================
        // SL-06: GET /api/admin/search-logs anonymous → 401
        // =========================================================

        [Fact]
        public async Task GetSearchLogs_Anonymous_Returns401()
        {
            // Arrange
            using HttpClient anonClient = _fixture.CreateClient();

            // Act
            HttpResponseMessage response = await anonClient.GetAdminSearchLogsAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                because: "unauthenticated requests to admin/search-logs must be rejected");
        }

        // =========================================================
        // SL-07: GET /api/admin/search-logs with Manager token → 403
        // =========================================================

        [Fact]
        public async Task GetSearchLogs_ManagerToken_Returns403()
        {
            // Arrange — Manager role is not allowed on search-logs (personal data; Admin-only per A-AU5)
            using HttpClient managerClient = await _fixture.CreateManagerClientAsync("sl07-mgr");

            // Act
            HttpResponseMessage response = await managerClient.GetAdminSearchLogsAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
                because: "GET /api/admin/search-logs is Admin-only (personal data risk — A-AU5 decision); Manager must be rejected");
        }

        // =========================================================
        // SL-08: GET /api/admin/search-logs term filter (case-insensitive) → only matching logs
        // =========================================================

        [Fact]
        public async Task GetSearchLogs_TermFilter_ReturnsOnlyMatchingLogs()
        {
            // Arrange — create two distinct search terms and generate a log for each
            string termA = $"FilterTermAlpha{Guid.NewGuid():N}";
            string termB = $"FilterTermBeta{Guid.NewGuid():N}";

            using HttpClient anonClient = _fixture.CreateClient();
            await anonClient.SearchBlogsAsync(termA);
            await anonClient.SearchBlogsAsync(termB);

            // Act — filter by termA (use mixed case to confirm case-insensitive match)
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            HttpResponseMessage logResp = await adminClient.GetAdminSearchLogsAsync(
                page: 1, pageSize: 50, term: termA.ToLowerInvariant());

            logResp.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument logDoc = JsonDocument.Parse(await logResp.Content.ReadAsStringAsync());
            JsonElement items = logDoc.RootElement.GetProperty("items");

            // Assert — all returned items must contain termA; termB must not appear
            bool termBFound = false;
            foreach (JsonElement item in items.EnumerateArray())
            {
                if (item.TryGetProperty("term", out JsonElement termProp))
                {
                    string? logTerm = termProp.GetString();
                    logTerm.Should().NotBeNull();
                    logTerm!.ToLowerInvariant().Should().Contain(termA.ToLowerInvariant(),
                        because: "filtered logs must match the given term filter (case-insensitive check)");

                    if (logTerm.Contains(termB, StringComparison.OrdinalIgnoreCase))
                    {
                        termBFound = true;
                    }
                }
            }

            termBFound.Should().BeFalse(
                because: "term filter must exclude logs that do not match termA");

            // At least one matching log must exist
            logDoc.RootElement.GetProperty("totalCount").GetInt32().Should().BeGreaterThan(0,
                because: "searching for termA must have created at least one matching log");
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static JsonElement? FindLogByTerm(JsonElement items, string term)
        {
            foreach (JsonElement item in items.EnumerateArray())
            {
                if (item.TryGetProperty("term", out JsonElement termProp) &&
                    string.Equals(termProp.GetString(), term, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }

            return null;
        }
    }
}
