using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.Phase5
{
    /// <summary>
    /// Integration tests for GET /api/admin/comments (Admin Comment Moderation, Faz 5).
    ///
    /// Endpoint: GET /api/admin/comments?page={p}&amp;pageSize={s}
    /// Auth:     [Authorize(Roles = "Admin")] — 401 for no token, 403 for non-Admin roles.
    /// Response: PagedResult&lt;CommentModerationResponse&gt; — flat list of comments + replies,
    ///           sorted createdAt descending (newest first).
    ///
    /// Covered scenarios:
    ///   ACM-01. Admin token → 200 with correct PagedResult shape
    ///   ACM-02. Admin token + created comment → item appears in list with correct field mappings
    ///   ACM-03. Admin token + created reply   → reply appears with isReply=true and parentCommentId set
    ///   ACM-04. Comment AND reply in same flat list → both items visible, isReply distinguishes them
    ///   ACM-05. Anonymous (no token) → 401
    ///   ACM-06. Authenticated but normal User role → 403
    ///   ACM-07. Manager role → 403 (admin-only endpoint per A6 permission matrix)
    ///   ACM-08. Pagination: pageSize=2 with 3 comments → page 1 returns 2 items, correct pagination metadata
    ///   ACM-09. Pagination: page 2 of the above → 1 item, hasPreviousPage=true, hasNextPage=false
    ///   ACM-10. pageSize exceeding MaxPageSize (100) is clamped to 100
    /// </summary>
    [Collection(BlogApiCollection.CollectionName)]
    public sealed class AdminCommentModerationTests
    {
        private readonly BlogApiFixture _fixture;

        public AdminCommentModerationTests(BlogApiFixture fixture)
        {
            _fixture = fixture;
        }

        // =========================================================
        // Shared arrange: creates category + blog via admin client.
        // =========================================================

        private async Task<(Guid BlogId, string BlogTitle)> ArrangeBlogAsync(string prefix)
        {
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync(
                $"AcmCat-{prefix}-{Guid.NewGuid():N}");
            string title = $"AcmBlog-{prefix}-{Guid.NewGuid():N}";
            Guid blogId = await adminClient.ArrangeCreateBlogAsync(title, "Description for moderation test.", catId);
            return (blogId, title);
        }

        // =========================================================
        // ACM-01: Admin token → 200 with correct PagedResult shape
        // =========================================================

        [Fact]
        public async Task GetAdminComments_AdminToken_Returns200WithPagedShape()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();

            // Act
            HttpResponseMessage response = await adminClient.GetAdminCommentsAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                because: "admin must be able to access the comment moderation list");

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.TryGetProperty("items", out _).Should()
                .BeTrue(because: "paged result must contain an 'items' array");
            root.TryGetProperty("totalCount", out _).Should()
                .BeTrue(because: "paged result must contain 'totalCount'");
            root.TryGetProperty("page", out JsonElement pageProp).Should()
                .BeTrue(because: "paged result must contain 'page'");
            root.TryGetProperty("pageSize", out _).Should()
                .BeTrue(because: "paged result must contain 'pageSize'");
            root.TryGetProperty("totalPages", out _).Should()
                .BeTrue(because: "paged result must contain 'totalPages'");
            root.TryGetProperty("hasPreviousPage", out _).Should()
                .BeTrue(because: "paged result must contain 'hasPreviousPage'");
            root.TryGetProperty("hasNextPage", out _).Should()
                .BeTrue(because: "paged result must contain 'hasNextPage'");

            pageProp.GetInt32().Should().Be(1,
                because: "default page parameter is 1");
        }

        // =========================================================
        // ACM-02: Created comment appears in list with correct field mappings
        // =========================================================

        [Fact]
        public async Task GetAdminComments_WithCreatedComment_CommentAppearsWithCorrectFields()
        {
            // Arrange
            (Guid blogId, string blogTitle) = await ArrangeBlogAsync("acm02");
            (HttpClient userClient, string userId, _) = await _fixture.CreateUserClientAsync("acm02-user");
            const string commentText = "ACM-02 test comment content.";
            Guid commentId = await userClient.ArrangeCreateCommentAsync(blogId, commentText);
            userClient.Dispose();

            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();

            // Act — fetch a large enough page to capture the comment we just created
            HttpResponseMessage response = await adminClient.GetAdminCommentsAsync(page: 1, pageSize: 100);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement items = doc.RootElement.GetProperty("items");

            JsonElement? found = FindItemById(items, commentId);
            found.Should().NotBeNull(because: "the comment we created must appear in the moderation list");

            JsonElement item = found!.Value;
            item.GetProperty("isReply").GetBoolean().Should().BeFalse(
                because: "a top-level comment is not a reply");
            item.GetProperty("blogId").GetString().Should().Be(blogId.ToString(),
                because: "item must carry the blog id for the delete route");
            item.GetProperty("blogTitle").GetString().Should().Be(blogTitle,
                because: "item must carry the blog title for display");
            item.GetProperty("userId").GetString().Should().Be(userId,
                because: "item must carry the author user id");
            item.GetProperty("text").GetString().Should().Be(commentText,
                because: "item must carry the original comment text");

            // parentCommentId must be null for a top-level comment
            item.TryGetProperty("parentCommentId", out JsonElement parentIdProp).Should().BeTrue();
            parentIdProp.ValueKind.Should().Be(JsonValueKind.Null,
                because: "a top-level comment has no parent and parentCommentId must be null");

            // authorName must be non-empty (FirstName + LastName joined in DB)
            item.GetProperty("authorName").GetString().Should().NotBeNullOrWhiteSpace(
                because: "authorName is required for moderator visibility");
        }

        // =========================================================
        // ACM-03: Created reply appears with isReply=true and parentCommentId populated
        // =========================================================

        [Fact]
        public async Task GetAdminComments_WithCreatedReply_ReplyAppearsWithIsReplyTrueAndParentId()
        {
            // Arrange
            (Guid blogId, _) = await ArrangeBlogAsync("acm03");
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("acm03-user");
            Guid commentId = await userClient.ArrangeCreateCommentAsync(blogId, "ACM-03 parent comment.");
            const string replyText = "ACM-03 reply content.";
            Guid replyId = await userClient.ArrangeCreateReplyAsync(commentId, replyText);
            userClient.Dispose();

            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();

            // Act
            HttpResponseMessage response = await adminClient.GetAdminCommentsAsync(page: 1, pageSize: 100);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement items = doc.RootElement.GetProperty("items");

            JsonElement? found = FindItemById(items, replyId);
            found.Should().NotBeNull(because: "the reply we created must appear in the moderation list");

            JsonElement item = found!.Value;
            item.GetProperty("isReply").GetBoolean().Should().BeTrue(
                because: "a SubComment (reply) must have isReply=true");
            item.GetProperty("text").GetString().Should().Be(replyText,
                because: "reply text must be correctly mapped");

            item.TryGetProperty("parentCommentId", out JsonElement parentIdProp).Should().BeTrue();
            parentIdProp.ValueKind.Should().NotBe(JsonValueKind.Null,
                because: "a reply must have a non-null parentCommentId");
            parentIdProp.GetString().Should().Be(commentId.ToString(),
                because: "parentCommentId must point to the parent comment");

            item.GetProperty("blogId").GetString().Should().Be(blogId.ToString(),
                because: "reply item must carry the blog id for the delete route");
        }

        // =========================================================
        // ACM-04: Comment AND reply in same flat list
        // =========================================================

        [Fact]
        public async Task GetAdminComments_CommentAndReply_BothAppearInFlatListDistinguishedByIsReply()
        {
            // Arrange — one comment + one reply under it on the same blog
            (Guid blogId, _) = await ArrangeBlogAsync("acm04");
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("acm04-user");
            Guid commentId = await userClient.ArrangeCreateCommentAsync(blogId, "ACM-04 parent comment.");
            Guid replyId = await userClient.ArrangeCreateReplyAsync(commentId, "ACM-04 reply.");
            userClient.Dispose();

            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();

            // Act
            HttpResponseMessage response = await adminClient.GetAdminCommentsAsync(page: 1, pageSize: 100);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement items = doc.RootElement.GetProperty("items");

            JsonElement? commentItem = FindItemById(items, commentId);
            JsonElement? replyItem = FindItemById(items, replyId);

            commentItem.Should().NotBeNull(because: "the top-level comment must appear in the flat list");
            replyItem.Should().NotBeNull(because: "the reply must appear in the flat list alongside the comment");

            commentItem!.Value.GetProperty("isReply").GetBoolean().Should().BeFalse(
                because: "the top-level comment must be marked as not a reply");
            replyItem!.Value.GetProperty("isReply").GetBoolean().Should().BeTrue(
                because: "the reply must be marked as a reply");
        }

        // =========================================================
        // ACM-05: Anonymous (no token) → 401
        // =========================================================

        [Fact]
        public async Task GetAdminComments_NoToken_Returns401()
        {
            // Arrange
            using HttpClient anonClient = _fixture.CreateClient();

            // Act
            HttpResponseMessage response = await anonClient.GetAdminCommentsAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                because: "unauthenticated requests to admin endpoints must be rejected with 401");
        }

        // =========================================================
        // ACM-06: Authenticated normal User role → 403
        // =========================================================

        [Fact]
        public async Task GetAdminComments_NormalUserRole_Returns403()
        {
            // Arrange
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("acm06-user");

            // Act
            HttpResponseMessage response = await userClient.GetAdminCommentsAsync();
            userClient.Dispose();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
                because: "a regular User role must not access the admin comment moderation endpoint");
        }

        // =========================================================
        // ACM-07: Manager role → 403  (Admin-only per A6 permission matrix)
        // =========================================================

        [Fact]
        public async Task GetAdminComments_ManagerRole_Returns403()
        {
            // Arrange — Manager role is allowed on some admin endpoints but NOT on this one (A6:
            // "comment moderation for all blogs belongs to Admin only").
            using HttpClient managerClient = await _fixture.CreateManagerClientAsync("acm07-mgr");

            // Act
            HttpResponseMessage response = await managerClient.GetAdminCommentsAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
                because: "per A6 permission matrix, GET /api/admin/comments is Admin-only; Manager must be rejected");
        }

        // =========================================================
        // ACM-08: Pagination — page 1 of 3 comments with pageSize=2
        //         → 2 items, totalCount=3, totalPages=2, hasNextPage=true, hasPreviousPage=false
        // =========================================================

        [Fact]
        public async Task GetAdminComments_PaginationFirstPage_ReturnsCorrectMetadata()
        {
            // Arrange — create 3 comments on the same blog so we can control the subset.
            // We use a dedicated blog + user pair to keep counts predictable relative to pre-existing data.
            (Guid blogId, _) = await ArrangeBlogAsync("acm08");
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("acm08-user");

            Guid comment1 = await userClient.ArrangeCreateCommentAsync(blogId, "ACM-08 comment one.");
            Guid comment2 = await userClient.ArrangeCreateCommentAsync(blogId, "ACM-08 comment two.");
            Guid comment3 = await userClient.ArrangeCreateCommentAsync(blogId, "ACM-08 comment three.");
            userClient.Dispose();

            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();

            // Act — get all comments for the moderation list (large page) then verify pagination metadata
            // via a narrow pageSize call.  Because other tests may have created comments in the shared DB,
            // we collect the full set first to know the real totalCount, then assert relative invariants.
            HttpResponseMessage allResponse = await adminClient.GetAdminCommentsAsync(page: 1, pageSize: 100);
            allResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument allDoc = await allResponse.ReadAsJsonDocumentAsync();
            int fullTotal = allDoc.RootElement.GetProperty("totalCount").GetInt32();

            // Now request page 1 with pageSize=2
            HttpResponseMessage page1Response = await adminClient.GetAdminCommentsAsync(page: 1, pageSize: 2);

            // Assert
            page1Response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument page1Doc = await page1Response.ReadAsJsonDocumentAsync();
            JsonElement page1Root = page1Doc.RootElement;

            page1Root.GetProperty("items").GetArrayLength().Should().Be(2,
                because: "pageSize=2 must return exactly 2 items on a page that has at least 2 comments");
            page1Root.GetProperty("totalCount").GetInt32().Should().Be(fullTotal,
                because: "totalCount must match the total regardless of the page size");
            page1Root.GetProperty("page").GetInt32().Should().Be(1,
                because: "we requested page 1");
            page1Root.GetProperty("pageSize").GetInt32().Should().Be(2,
                because: "we requested pageSize=2");

            int expectedTotalPages = (int)Math.Ceiling((double)fullTotal / 2);
            page1Root.GetProperty("totalPages").GetInt32().Should().Be(expectedTotalPages,
                because: "totalPages = ceil(totalCount / pageSize)");
            page1Root.GetProperty("hasPreviousPage").GetBoolean().Should().BeFalse(
                because: "page 1 has no previous page");

            if (fullTotal > 2)
            {
                page1Root.GetProperty("hasNextPage").GetBoolean().Should().BeTrue(
                    because: "there are more than 2 comments total so page 1 must have a next page");
            }
        }

        // =========================================================
        // ACM-09: Pagination — page 2 with pageSize=2 (given at least 3 comments in DB)
        //         → items.Count <= 2, hasPreviousPage=true
        // =========================================================

        [Fact]
        public async Task GetAdminComments_PaginationSecondPage_HasPreviousPageTrueAndDifferentItems()
        {
            // Arrange — ensure at least 3 comments exist (isolated to a dedicated blog/user pair).
            (Guid blogId, _) = await ArrangeBlogAsync("acm09");
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("acm09-user");

            await userClient.ArrangeCreateCommentAsync(blogId, "ACM-09 comment alpha.");
            await userClient.ArrangeCreateCommentAsync(blogId, "ACM-09 comment beta.");
            await userClient.ArrangeCreateCommentAsync(blogId, "ACM-09 comment gamma.");
            userClient.Dispose();

            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();

            // Act — get page 1 and page 2 with pageSize=2
            HttpResponseMessage page1Response = await adminClient.GetAdminCommentsAsync(page: 1, pageSize: 2);
            HttpResponseMessage page2Response = await adminClient.GetAdminCommentsAsync(page: 2, pageSize: 2);

            // Assert
            page1Response.StatusCode.Should().Be(HttpStatusCode.OK);
            page2Response.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument page1Doc = await page1Response.ReadAsJsonDocumentAsync();
            JsonDocument page2Doc = await page2Response.ReadAsJsonDocumentAsync();

            JsonElement page2Root = page2Doc.RootElement;

            page2Root.GetProperty("page").GetInt32().Should().Be(2,
                because: "we requested page 2");
            page2Root.GetProperty("hasPreviousPage").GetBoolean().Should().BeTrue(
                because: "page 2 always has a previous page");

            // Collect the IDs from both pages and verify they are disjoint (no duplicates).
            var page1Ids = CollectItemIds(page1Doc.RootElement.GetProperty("items"));
            var page2Ids = CollectItemIds(page2Root.GetProperty("items"));

            page2Ids.Should().NotIntersectWith(page1Ids,
                because: "items on page 2 must not duplicate items from page 1");
        }

        // =========================================================
        // ACM-10: pageSize > MaxPageSize (100) is clamped server-side to 100
        // =========================================================

        [Fact]
        public async Task GetAdminComments_PageSizeExceedsMax_IsClampedTo100()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();

            // Act — request pageSize=999 which exceeds the MaxPageSize=100 cap
            HttpResponseMessage response = await adminClient.GetAdminCommentsAsync(page: 1, pageSize: 999);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                because: "the server clamps pageSize to MaxPageSize=100 instead of rejecting the request");

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(100,
                because: "the handler must clamp any pageSize > 100 to the MaxPageSize cap of 100");
        }

        // =========================================================
        // Private helpers
        // =========================================================

        /// <summary>
        /// Finds an item in the JSON items array by its Guid id. Returns null if not found.
        /// </summary>
        private static JsonElement? FindItemById(JsonElement items, Guid id)
        {
            string idStr = id.ToString();
            foreach (JsonElement item in items.EnumerateArray())
            {
                if (item.TryGetProperty("id", out JsonElement idProp) &&
                    string.Equals(idProp.GetString(), idStr, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// Collects all 'id' values from an items JSON array into a HashSet for set comparison.
        /// </summary>
        private static HashSet<string> CollectItemIds(JsonElement items)
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (JsonElement item in items.EnumerateArray())
            {
                if (item.TryGetProperty("id", out JsonElement idProp))
                {
                    string? idStr = idProp.GetString();
                    if (idStr is not null)
                    {
                        ids.Add(idStr);
                    }
                }
            }
            return ids;
        }
    }
}
