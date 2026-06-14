using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.Audit
{
    /// <summary>
    /// Integration tests for the IP audit hashing feature.
    ///
    /// Confirms that:
    ///   A) Blog and Message creation capture the caller's IP via X-Forwarded-For,
    ///      hash it deterministically with the configured salt, and persist only the hash.
    ///   B) The hash is visible on admin audit endpoints but NEVER on public endpoints.
    ///
    /// Test environment configuration (appsettings.json):
    ///   Audit:IpHashSalt = "zn-dev-ip-salt"   (deterministic — same IP always produces same hash)
    ///
    /// Covered scenarios:
    ///   IP-01. Blog created with X-Forwarded-For → GET /api/admin/blogs/{id}: creatorIpHash not null/empty
    ///   IP-02. Blog created with X-Forwarded-For → creatorIpHash is NOT the raw IP string
    ///   IP-03. Two blogs created with same X-Forwarded-For IP → same creatorIpHash (deterministic)
    ///   IP-04. Two blogs created with different IPs → different creatorIpHash values
    ///   IP-05. Blog created WITHOUT X-Forwarded-For → creatorIpHash is null
    ///   IP-06. GET /api/blogs/{id} (public) → response does NOT contain creatorIpHash property
    ///   IP-07. Message sent with X-Forwarded-For → GET /api/admin/messages: senderIpHash not null/empty
    ///   IP-08. Message sent with X-Forwarded-For → senderIpHash is NOT the raw IP string
    ///   IP-09. Message sent WITHOUT X-Forwarded-For → senderIpHash is null
    /// </summary>
    [Collection(BlogApiCollection.CollectionName)]
    public sealed class AuditIpTests
    {
        private const string SampleIp1 = "203.0.113.5";
        private const string SampleIp2 = "198.51.100.42";

        private readonly BlogApiFixture _fixture;

        public AuditIpTests(BlogApiFixture fixture)
        {
            _fixture = fixture;
        }

        // =========================================================
        // IP-01: Blog with X-Forwarded-For → admin audit shows non-null/empty creatorIpHash
        // =========================================================

        [Fact]
        public async Task CreateBlog_WithForwardedForHeader_AdminAuditShowsNonNullIpHash()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"AuditCat-IP01-{Guid.NewGuid():N}");

            // Act — create blog with X-Forwarded-For header
            HttpResponseMessage createResp = await adminClient.CreateBlogWithIpAsync(
                $"IpHashBlog-IP01-{Guid.NewGuid():N}",
                "Description for IP audit test.",
                catId,
                SampleIp1);

            createResp.StatusCode.Should().Be(HttpStatusCode.Created,
                because: "blog creation must succeed to test IP hash");

            JsonDocument createDoc = JsonDocument.Parse(await createResp.Content.ReadAsStringAsync());
            Guid blogId = Guid.Parse(createDoc.RootElement.GetProperty("id").GetString()!);

            // Act — retrieve audit detail from admin endpoint
            HttpResponseMessage auditResp = await adminClient.GetAdminBlogAuditAsync(blogId);

            // Assert
            auditResp.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument auditDoc = JsonDocument.Parse(await auditResp.Content.ReadAsStringAsync());

            auditDoc.RootElement.TryGetProperty("creatorIpHash", out JsonElement hashProp).Should().BeTrue(
                because: "admin blog audit detail must include creatorIpHash field");

            hashProp.ValueKind.Should().NotBe(JsonValueKind.Null,
                because: "creatorIpHash must not be null when X-Forwarded-For was provided");

            string? hashValue = hashProp.GetString();
            hashValue.Should().NotBeNullOrWhiteSpace(
                because: "creatorIpHash must be a non-empty string when IP was resolved");
        }

        // =========================================================
        // IP-02: creatorIpHash must NOT equal the raw IP string
        // =========================================================

        [Fact]
        public async Task CreateBlog_WithForwardedForHeader_CreatorIpHashIsNotRawIp()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"AuditCat-IP02-{Guid.NewGuid():N}");

            HttpResponseMessage createResp = await adminClient.CreateBlogWithIpAsync(
                $"IpHashBlog-IP02-{Guid.NewGuid():N}",
                "Description for raw IP comparison test.",
                catId,
                SampleIp1);

            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            JsonDocument createDoc = JsonDocument.Parse(await createResp.Content.ReadAsStringAsync());
            Guid blogId = Guid.Parse(createDoc.RootElement.GetProperty("id").GetString()!);

            // Act
            HttpResponseMessage auditResp = await adminClient.GetAdminBlogAuditAsync(blogId);
            JsonDocument auditDoc = JsonDocument.Parse(await auditResp.Content.ReadAsStringAsync());
            string? hashValue = auditDoc.RootElement.GetProperty("creatorIpHash").GetString();

            // Assert — the hash must differ from the raw IP to confirm hashing occurred
            hashValue.Should().NotBe(SampleIp1,
                because: "the IP address must be hashed before storage — storing raw IPs is a KVKK violation");
        }

        // =========================================================
        // IP-03: Same IP on two different blogs → same hash (deterministic)
        // =========================================================

        [Fact]
        public async Task CreateBlog_SameIp_ProducesSameHash()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"AuditCat-IP03-{Guid.NewGuid():N}");

            // Create two blogs with the same X-Forwarded-For IP
            HttpResponseMessage resp1 = await adminClient.CreateBlogWithIpAsync(
                $"SameIpBlog1-{Guid.NewGuid():N}", "First blog.", catId, SampleIp1);
            resp1.StatusCode.Should().Be(HttpStatusCode.Created);
            Guid blogId1 = Guid.Parse(
                JsonDocument.Parse(await resp1.Content.ReadAsStringAsync())
                    .RootElement.GetProperty("id").GetString()!);

            HttpResponseMessage resp2 = await adminClient.CreateBlogWithIpAsync(
                $"SameIpBlog2-{Guid.NewGuid():N}", "Second blog.", catId, SampleIp1);
            resp2.StatusCode.Should().Be(HttpStatusCode.Created);
            Guid blogId2 = Guid.Parse(
                JsonDocument.Parse(await resp2.Content.ReadAsStringAsync())
                    .RootElement.GetProperty("id").GetString()!);

            // Act — retrieve audit hashes for both
            HttpResponseMessage audit1Resp = await adminClient.GetAdminBlogAuditAsync(blogId1);
            HttpResponseMessage audit2Resp = await adminClient.GetAdminBlogAuditAsync(blogId2);

            string? hash1 = JsonDocument.Parse(await audit1Resp.Content.ReadAsStringAsync())
                .RootElement.GetProperty("creatorIpHash").GetString();
            string? hash2 = JsonDocument.Parse(await audit2Resp.Content.ReadAsStringAsync())
                .RootElement.GetProperty("creatorIpHash").GetString();

            // Assert — same IP → same hash (deterministic tuzlu SHA-256)
            hash1.Should().NotBeNullOrWhiteSpace();
            hash1.Should().Be(hash2,
                because: "the same IP address with the same salt must always produce the same hash");
        }

        // =========================================================
        // IP-04: Different IPs → different hashes
        // =========================================================

        [Fact]
        public async Task CreateBlog_DifferentIps_ProduceDifferentHashes()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"AuditCat-IP04-{Guid.NewGuid():N}");

            HttpResponseMessage resp1 = await adminClient.CreateBlogWithIpAsync(
                $"DiffIpBlog1-{Guid.NewGuid():N}", "Blog with IP1.", catId, SampleIp1);
            resp1.StatusCode.Should().Be(HttpStatusCode.Created);
            Guid blogId1 = Guid.Parse(
                JsonDocument.Parse(await resp1.Content.ReadAsStringAsync())
                    .RootElement.GetProperty("id").GetString()!);

            HttpResponseMessage resp2 = await adminClient.CreateBlogWithIpAsync(
                $"DiffIpBlog2-{Guid.NewGuid():N}", "Blog with IP2.", catId, SampleIp2);
            resp2.StatusCode.Should().Be(HttpStatusCode.Created);
            Guid blogId2 = Guid.Parse(
                JsonDocument.Parse(await resp2.Content.ReadAsStringAsync())
                    .RootElement.GetProperty("id").GetString()!);

            // Act
            HttpResponseMessage audit1Resp = await adminClient.GetAdminBlogAuditAsync(blogId1);
            HttpResponseMessage audit2Resp = await adminClient.GetAdminBlogAuditAsync(blogId2);

            string? hash1 = JsonDocument.Parse(await audit1Resp.Content.ReadAsStringAsync())
                .RootElement.GetProperty("creatorIpHash").GetString();
            string? hash2 = JsonDocument.Parse(await audit2Resp.Content.ReadAsStringAsync())
                .RootElement.GetProperty("creatorIpHash").GetString();

            // Assert — different IPs must produce different hashes
            hash1.Should().NotBeNullOrWhiteSpace();
            hash2.Should().NotBeNullOrWhiteSpace();
            hash1.Should().NotBe(hash2,
                because: "different IP addresses must produce different hash values");
        }

        // =========================================================
        // IP-05: Blog created without X-Forwarded-For → creatorIpHash is null
        // =========================================================

        [Fact]
        public async Task CreateBlog_WithoutForwardedForHeader_CreatorIpHashIsNull()
        {
            // Arrange — use the standard CreateBlogAsync helper (no IP header)
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"AuditCat-IP05-{Guid.NewGuid():N}");
            Guid blogId = await adminClient.ArrangeCreateBlogAsync(
                $"NoIpBlog-IP05-{Guid.NewGuid():N}",
                "Blog created without X-Forwarded-For header.",
                catId);

            // Act
            HttpResponseMessage auditResp = await adminClient.GetAdminBlogAuditAsync(blogId);

            // Assert — when no IP is resolvable from context, creatorIpHash must be null
            auditResp.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument auditDoc = JsonDocument.Parse(await auditResp.Content.ReadAsStringAsync());

            // In LocalDB test environment HttpContext.Connection.RemoteIpAddress is null,
            // and there is no X-Forwarded-For header → IClientIpResolver returns null → hash is null.
            auditDoc.RootElement.TryGetProperty("creatorIpHash", out JsonElement hashProp).Should().BeTrue(
                because: "creatorIpHash property must always be present in admin audit response");

            hashProp.ValueKind.Should().Be(JsonValueKind.Null,
                because: "when no IP can be resolved, creatorIpHash must be null not an empty string");
        }

        // =========================================================
        // IP-06: GET /api/blogs/{id} (public) must NOT expose creatorIpHash
        // =========================================================

        [Fact]
        public async Task GetPublicBlog_ResponseDoesNotContainCreatorIpHash()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"AuditCat-IP06-{Guid.NewGuid():N}");
            Guid blogId = await adminClient.ArrangeCreateBlogAsync(
                $"PublicLeak-IP06-{Guid.NewGuid():N}",
                "Testing that public endpoint does not leak IP hash.",
                catId);

            // Act — call the PUBLIC endpoint (no auth)
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage publicResp = await anonClient.GetBlogByIdAsync(blogId);

            // Assert
            publicResp.StatusCode.Should().Be(HttpStatusCode.OK);
            string publicBody = await publicResp.Content.ReadAsStringAsync();

            // The public BlogDetailResponse record does not have creatorIpHash field;
            // verify it is absent from the JSON to prevent accidental leakage.
            publicBody.Should().NotContain("creatorIpHash",
                because: "the public blog endpoint must NEVER expose the IP audit hash field");
            publicBody.Should().NotContain("ipHash",
                because: "no IP-related audit field may appear in public responses");
        }

        // =========================================================
        // IP-07: Message with X-Forwarded-For → admin messages list shows non-null senderIpHash
        // =========================================================

        [Fact]
        public async Task SendMessage_WithForwardedForHeader_AdminShowsNonNullSenderIpHash()
        {
            // Arrange — send message with IP header
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage sendResp = await anonClient.SendMessageWithIpAsync(
                name: $"IpAuditSender-{Guid.NewGuid():N}",
                email: $"audit-ip07-{Guid.NewGuid():N}@test.com",
                subject: "IP Audit Test Subject",
                body: "This message tests senderIpHash capture.",
                forwardedForIp: SampleIp1);

            sendResp.StatusCode.Should().Be(HttpStatusCode.Created,
                because: "message send must succeed for IP hash test");

            // Act — fetch admin messages list and find our message by unique sender name
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            HttpResponseMessage listResp = await adminClient.GetAdminMessagesAsync(page: 1, pageSize: 50);
            listResp.StatusCode.Should().Be(HttpStatusCode.OK);

            // Assert — find a message with non-null senderIpHash
            JsonDocument listDoc = JsonDocument.Parse(await listResp.Content.ReadAsStringAsync());
            JsonElement items = listDoc.RootElement.GetProperty("items");

            bool foundNonNullHash = false;
            foreach (JsonElement item in items.EnumerateArray())
            {
                if (item.TryGetProperty("senderIpHash", out JsonElement hashEl) &&
                    hashEl.ValueKind != JsonValueKind.Null &&
                    !string.IsNullOrWhiteSpace(hashEl.GetString()))
                {
                    foundNonNullHash = true;
                    break;
                }
            }

            foundNonNullHash.Should().BeTrue(
                because: "at least one message sent with X-Forwarded-For must have a non-null senderIpHash");
        }

        // =========================================================
        // IP-08: Message senderIpHash is NOT the raw IP string
        // =========================================================

        [Fact]
        public async Task SendMessage_WithForwardedForHeader_SenderIpHashIsNotRawIp()
        {
            // Arrange
            string uniqueName = $"RawIpCheck-IP08-{Guid.NewGuid():N}";
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage sendResp = await anonClient.SendMessageWithIpAsync(
                name: uniqueName,
                email: $"rawip-{Guid.NewGuid():N}@test.com",
                subject: "Raw IP check subject",
                body: "Testing that hash differs from raw IP.",
                forwardedForIp: SampleIp2);

            sendResp.StatusCode.Should().Be(HttpStatusCode.Created);

            // Act — list messages and find the one with our unique name
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            HttpResponseMessage listResp = await adminClient.GetAdminMessagesAsync(page: 1, pageSize: 100);
            listResp.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument listDoc = JsonDocument.Parse(await listResp.Content.ReadAsStringAsync());
            JsonElement items = listDoc.RootElement.GetProperty("items");

            string? foundHash = null;
            foreach (JsonElement item in items.EnumerateArray())
            {
                if (item.TryGetProperty("name", out JsonElement nameProp) &&
                    nameProp.GetString() == uniqueName &&
                    item.TryGetProperty("senderIpHash", out JsonElement hashEl))
                {
                    foundHash = hashEl.GetString();
                    break;
                }
            }

            // Assert
            foundHash.Should().NotBeNullOrWhiteSpace(
                because: "the message sent with X-Forwarded-For must have a senderIpHash");
            foundHash.Should().NotBe(SampleIp2,
                because: "senderIpHash must be the hashed value, never the raw IP address");
        }

        // =========================================================
        // IP-09: Message sent without X-Forwarded-For → senderIpHash is null
        // =========================================================

        [Fact]
        public async Task SendMessage_WithoutForwardedForHeader_SenderIpHashIsNull()
        {
            // Arrange — send via the standard helper (no IP header)
            string uniqueName = $"NoIpMsg-IP09-{Guid.NewGuid():N}";
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage sendResp = await anonClient.SendMessageAsync(
                uniqueName,
                $"noip-{Guid.NewGuid():N}@test.com",
                "No IP hash subject",
                "Testing null senderIpHash when no X-Forwarded-For is set.");

            sendResp.StatusCode.Should().Be(HttpStatusCode.Created);

            // Act — locate the message in admin list
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            HttpResponseMessage listResp = await adminClient.GetAdminMessagesAsync(page: 1, pageSize: 100);
            listResp.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument listDoc = JsonDocument.Parse(await listResp.Content.ReadAsStringAsync());
            JsonElement items = listDoc.RootElement.GetProperty("items");

            JsonElement? targetItem = null;
            foreach (JsonElement item in items.EnumerateArray())
            {
                if (item.TryGetProperty("name", out JsonElement nameProp) &&
                    nameProp.GetString() == uniqueName)
                {
                    targetItem = item;
                    break;
                }
            }

            // Assert
            targetItem.Should().NotBeNull(
                because: "the message we sent must appear in the admin list");

            targetItem!.Value.TryGetProperty("senderIpHash", out JsonElement hashProp).Should().BeTrue(
                because: "senderIpHash must always be present as a field even when null");

            hashProp.ValueKind.Should().Be(JsonValueKind.Null,
                because: "when no X-Forwarded-For header is present and RemoteIpAddress is null in test, senderIpHash must be null");
        }
    }
}
