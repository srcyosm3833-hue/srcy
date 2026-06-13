using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.Comments
{
    /// <summary>
    /// Integration tests for Comment endpoints (Faz 3 – Dilim A).
    ///
    /// Covered scenarios:
    ///   C-1.  GET /api/blogs/{blogId}/comments (anonymous) → 200, empty items on fresh blog
    ///   C-2.  GET /api/blogs/{blogId}/comments with comments → subCommentCount present and correct
    ///   C-3.  POST /api/blogs/{blogId}/comments authenticated → 201, isEdited:false
    ///   C-4.  POST without token → 401
    ///   C-5.  POST with non-existent blogId → 404
    ///   C-6.  POST with empty commentText → 400
    ///   C-7.  POST with commentText exceeding 1000 chars → 400
    ///   C-8.  PUT own comment → 200, isEdited:true
    ///   C-9.  PUT another user's comment (not owner, not admin) → 403
    ///   C-10. PUT by admin on someone else's comment → 403 (admin cannot edit others)
    ///   C-11. DELETE own comment → 204
    ///   C-12. DELETE another user's comment by admin → 204 (admin CAN delete)
    ///   C-13. DELETE blog author's attempt to delete commenter's comment → 403 (CRITICAL)
    ///   C-14. PUT non-existent comment id → 404 (before 403)
    ///   C-15. DELETE non-existent comment id → 404 (before 403)
    /// </summary>
    [Collection(BlogApiCollection.CollectionName)]
    public sealed class CommentTests
    {
        private readonly BlogApiFixture _fixture;

        public CommentTests(BlogApiFixture fixture)
        {
            _fixture = fixture;
        }

        // =========================================================
        // C-1: GET anonymous, no comments → 200, empty items
        // =========================================================

        [Fact]
        public async Task GetComments_AnonymousOnEmptyBlog_Returns200WithEmptyItems()
        {
            // Arrange — create a blog with no comments
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Cmnt-C1-{Guid.NewGuid():N}");

            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-C1-{Guid.NewGuid():N}", "Desc", catId);

            // Act
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.GetCommentsAsync(blogId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.TryGetProperty("items", out JsonElement itemsProp).Should().BeTrue();
            itemsProp.GetArrayLength().Should().Be(0, because: "no comments have been added yet");
            root.TryGetProperty("totalCount", out JsonElement totalCountProp).Should().BeTrue();
            totalCountProp.GetInt32().Should().Be(0);
        }

        // =========================================================
        // C-2: GET with comments → subCommentCount is correct
        // =========================================================

        [Fact]
        public async Task GetComments_WithCommentsAndReplies_SubCommentCountIsCorrect()
        {
            // Arrange — blog + comment + 2 replies
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Cmnt-C2-{Guid.NewGuid():N}");

            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-C2-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("cmnt-c2-user");
            Guid commentId = await userClient.ArrangeCreateCommentAsync(blogId, "Main comment C2.");

            // Add 2 replies to this comment
            await userClient.ArrangeCreateReplyAsync(commentId, "Reply one.");
            await userClient.ArrangeCreateReplyAsync(commentId, "Reply two.");

            // Act
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.GetCommentsAsync(blogId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement items = doc.RootElement.GetProperty("items");

            bool foundComment = false;
            foreach (JsonElement item in items.EnumerateArray())
            {
                string? itemId = item.GetProperty("id").GetString();
                if (itemId == commentId.ToString())
                {
                    item.TryGetProperty("subCommentCount", out JsonElement subCountProp)
                        .Should().BeTrue("subCommentCount must be present in each comment item");
                    subCountProp.GetInt32().Should().Be(2,
                        because: "we added exactly 2 replies to this comment");
                    foundComment = true;
                    break;
                }
            }

            foundComment.Should().BeTrue("the comment we created must appear in the list");
        }

        // =========================================================
        // C-3: POST authenticated → 201, isEdited:false on creation
        // =========================================================

        [Fact]
        public async Task CreateComment_AuthenticatedUser_Returns201AndIsEditedFalse()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Cmnt-C3-{Guid.NewGuid():N}");

            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-C3-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("cmnt-c3-user");

            // Act
            HttpResponseMessage response = await userClient.CreateCommentAsync(blogId, "Hello from C3 test.");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.TryGetProperty("id", out _).Should().BeTrue();
            root.TryGetProperty("commentText", out JsonElement textProp).Should().BeTrue();
            textProp.GetString().Should().Be("Hello from C3 test.");

            root.TryGetProperty("isEdited", out JsonElement isEditedProp).Should().BeTrue();
            isEditedProp.GetBoolean().Should().BeFalse(
                because: "a newly created comment has never been updated — isEdited must be false");

            root.TryGetProperty("updatedAt", out JsonElement updatedAtProp).Should().BeTrue();
            updatedAtProp.ValueKind.Should().Be(JsonValueKind.Null,
                because: "updatedAt must be null on creation");
        }

        // =========================================================
        // C-4: POST without token → 401
        // =========================================================

        [Fact]
        public async Task CreateComment_NoToken_Returns401()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Cmnt-C4-{Guid.NewGuid():N}");
            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-C4-{Guid.NewGuid():N}", "Desc", catId);

            // Act — anonymous client attempts to create a comment
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.CreateCommentAsync(blogId, "Should fail.");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // =========================================================
        // C-5: POST with non-existent blogId → 404
        // =========================================================

        [Fact]
        public async Task CreateComment_NonExistentBlogId_Returns404()
        {
            // Arrange
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("cmnt-c5-user");
            Guid fakeBlogId = Guid.NewGuid();

            // Act
            HttpResponseMessage response = await userClient.CreateCommentAsync(fakeBlogId, "Should get 404.");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // =========================================================
        // C-6: POST with empty commentText → 400
        // =========================================================

        [Fact]
        public async Task CreateComment_EmptyCommentText_Returns400()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Cmnt-C6-{Guid.NewGuid():N}");
            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-C6-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("cmnt-c6-user");

            // Act
            HttpResponseMessage response = await userClient.CreateCommentAsync(blogId, string.Empty);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // =========================================================
        // C-7: POST with commentText > 1000 chars → 400
        // =========================================================

        [Fact]
        public async Task CreateComment_CommentTextExceeds1000Chars_Returns400()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Cmnt-C7-{Guid.NewGuid():N}");
            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-C7-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("cmnt-c7-user");

            string tooLongText = new string('x', 1001); // 1 character over the 1000-char limit

            // Act
            HttpResponseMessage response = await userClient.CreateCommentAsync(blogId, tooLongText);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // =========================================================
        // C-8: PUT own comment → 200, isEdited:true
        // =========================================================

        [Fact]
        public async Task UpdateComment_ByOwner_Returns200AndIsEditedTrue()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Cmnt-C8-{Guid.NewGuid():N}");
            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-C8-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient ownerClient, _, _) = await _fixture.CreateUserClientAsync("cmnt-c8-owner");
            Guid commentId = await ownerClient.ArrangeCreateCommentAsync(blogId, "Original text.");

            // Act
            HttpResponseMessage response = await ownerClient.UpdateCommentAsync(
                blogId, commentId, "Updated text.");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.GetProperty("isEdited").GetBoolean().Should().BeTrue(
                because: "after an update, isEdited must be true (UpdatedAt is set)");
            root.GetProperty("commentText").GetString().Should().Be("Updated text.");
            root.TryGetProperty("updatedAt", out JsonElement updatedAtProp).Should().BeTrue();
            updatedAtProp.ValueKind.Should().NotBe(JsonValueKind.Null,
                because: "updatedAt must be set after an update");
        }

        // =========================================================
        // C-9: PUT another user's comment (non-owner, non-admin) → 403
        // =========================================================

        [Fact]
        public async Task UpdateComment_ByNonOwnerNonAdmin_Returns403()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Cmnt-C9-{Guid.NewGuid():N}");
            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-C9-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient ownerClient, _, _) = await _fixture.CreateUserClientAsync("cmnt-c9-owner");
            Guid commentId = await ownerClient.ArrangeCreateCommentAsync(blogId, "Owner's comment.");

            (HttpClient attackerClient, _, _) = await _fixture.CreateUserClientAsync("cmnt-c9-attacker");

            // Act
            HttpResponseMessage response = await attackerClient.UpdateCommentAsync(
                blogId, commentId, "Hacked text.");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
                because: "only the comment owner may edit their comment");
        }

        // =========================================================
        // C-10 (CRITICAL): PUT by admin on someone else's comment → 403
        // =========================================================

        [Fact]
        public async Task UpdateComment_ByAdmin_Returns403()
        {
            // Arrange — user creates a comment; admin tries to edit it
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Cmnt-C10-{Guid.NewGuid():N}");
            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-C10-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("cmnt-c10-user");
            Guid commentId = await userClient.ArrangeCreateCommentAsync(blogId, "User's comment.");

            // Act — admin (not owner) attempts to edit
            HttpResponseMessage response = await adminClient.UpdateCommentAsync(
                blogId, commentId, "Admin-forced text.");

            // Assert — even admin cannot edit someone else's comment
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
                because: "admin cannot edit another user's comment — only the owner may edit");
        }

        // =========================================================
        // C-11: DELETE own comment → 204
        // =========================================================

        [Fact]
        public async Task DeleteComment_ByOwner_Returns204()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Cmnt-C11-{Guid.NewGuid():N}");
            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-C11-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient ownerClient, _, _) = await _fixture.CreateUserClientAsync("cmnt-c11-owner");
            Guid commentId = await ownerClient.ArrangeCreateCommentAsync(blogId, "To be deleted by owner.");

            // Act
            HttpResponseMessage response = await ownerClient.DeleteCommentAsync(blogId, commentId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        // =========================================================
        // C-12: DELETE another user's comment by admin → 204
        // =========================================================

        [Fact]
        public async Task DeleteComment_ByAdmin_Returns204()
        {
            // Arrange — user creates a comment; admin deletes it
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Cmnt-C12-{Guid.NewGuid():N}");
            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-C12-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("cmnt-c12-user");
            Guid commentId = await userClient.ArrangeCreateCommentAsync(blogId, "To be deleted by admin.");

            // Act
            HttpResponseMessage response = await adminClient.DeleteCommentAsync(blogId, commentId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent,
                because: "admin must be able to delete any comment regardless of ownership");
        }

        // =========================================================
        // C-13 (CRITICAL): Blog author deletes commenter's comment → 403
        // Blog writer has no special comment deletion privilege.
        // =========================================================

        [Fact]
        public async Task DeleteComment_ByBlogAuthorWhoIsNotCommentOwnerOrAdmin_Returns403()
        {
            // Arrange:
            //   - userA creates the blog
            //   - userB posts a comment on that blog
            //   - userA (blog author, neither comment owner nor admin) tries to delete userB's comment
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Cmnt-C13-{Guid.NewGuid():N}");

            // Faz 5: blog creation requires Admin or Manager role.
            // A plain User (blogAuthorClient) acts as the blog "author" in the social sense,
            // but they cannot actually create the blog (Manager needed). We use adminClient to
            // create the blog, then a separate User acts as someone who would be the "blog manager"
            // but is not the comment owner. The scenario remains valid: a non-comment-owner, non-admin
            // User attempts to delete a commenter's comment and must get 403.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync(
                $"Blog-C13-{Guid.NewGuid():N}", "Desc", catId);

            (HttpClient blogAuthorClient, _, _) = await _fixture.CreateUserClientAsync("cmnt-c13-blogauthor");
            (HttpClient commenterClient, _, _) = await _fixture.CreateUserClientAsync("cmnt-c13-commenter");
            Guid commentId = await commenterClient.ArrangeCreateCommentAsync(blogId, "Commenter's message.");

            // Act — blogAuthorClient (a plain User, neither comment owner nor admin) attempts to delete commenter's comment
            HttpResponseMessage response = await blogAuthorClient.DeleteCommentAsync(blogId, commentId);

            // Assert — blog author has no special privilege; only comment owner or admin may delete
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
                because: "blog authorship does not grant comment deletion rights — only the comment owner or an admin may delete");
        }

        // =========================================================
        // C-14: PUT non-existent comment id → 404 (not 403)
        // =========================================================

        [Fact]
        public async Task UpdateComment_NonExistentId_Returns404NotForbidden()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Cmnt-C14-{Guid.NewGuid():N}");
            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-C14-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("cmnt-c14-user");
            Guid fakeCommentId = Guid.NewGuid();

            // Act
            HttpResponseMessage response = await userClient.UpdateCommentAsync(
                blogId, fakeCommentId, "Some text.");

            // Assert — handler checks NotFound before Forbidden
            response.StatusCode.Should().Be(HttpStatusCode.NotFound,
                because: "handler must return 404 before 403 to avoid leaking resource existence");
        }

        // =========================================================
        // C-15: DELETE non-existent comment id → 404 (not 403)
        // =========================================================

        [Fact]
        public async Task DeleteComment_NonExistentId_Returns404NotForbidden()
        {
            // Arrange
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("cmnt-c15-user");
            Guid fakeBlogId = Guid.NewGuid();
            Guid fakeCommentId = Guid.NewGuid();

            // Act
            HttpResponseMessage response = await userClient.DeleteCommentAsync(fakeBlogId, fakeCommentId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound,
                because: "handler must return 404 before checking authorization");
        }
    }
}
