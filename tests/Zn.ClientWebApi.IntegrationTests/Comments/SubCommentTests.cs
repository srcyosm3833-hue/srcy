using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.Comments
{
    /// <summary>
    /// Integration tests for SubComment (Reply) endpoints (Faz 3 – Dilim A).
    ///
    /// Covered scenarios:
    ///   SC-1.  POST reply authenticated → 201, isEdited:false
    ///   SC-2.  POST reply without token → 401
    ///   SC-3.  POST reply to non-existent commentId → 404
    ///   SC-4.  POST reply with empty text → 400
    ///   SC-5.  POST reply with text exceeding 1000 chars → 400
    ///   SC-6.  PUT own reply → 200, isEdited:true
    ///   SC-7.  PUT another user's reply (non-owner, non-admin) → 403
    ///   SC-8.  DELETE own reply → 204
    ///   SC-9.  DELETE reply by admin (not owner) → 204
    ///   SC-10. DELETE reply by another normal user → 403
    ///   SC-11. Cascade: parent comment deleted → reply POST to orphaned comment → 404
    /// </summary>
    [Collection(BlogApiCollection.CollectionName)]
    public sealed class SubCommentTests
    {
        private readonly BlogApiFixture _fixture;

        public SubCommentTests(BlogApiFixture fixture)
        {
            _fixture = fixture;
        }

        // =========================================================
        // SC-1: POST reply authenticated → 201, isEdited:false
        // =========================================================

        [Fact]
        public async Task CreateReply_AuthenticatedUser_Returns201AndIsEditedFalse()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-SC1-{Guid.NewGuid():N}");
            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-SC1-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("sc1-user");
            Guid commentId = await userClient.ArrangeCreateCommentAsync(blogId, "Parent comment SC1.");

            // Act
            HttpResponseMessage response = await userClient.CreateReplyAsync(commentId, "My reply SC1.");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.TryGetProperty("id", out _).Should().BeTrue();
            root.GetProperty("subCommentText").GetString().Should().Be("My reply SC1.");
            root.GetProperty("isEdited").GetBoolean().Should().BeFalse(
                because: "a newly created reply has never been updated — isEdited must be false");
            root.GetProperty("updatedAt").ValueKind.Should().Be(JsonValueKind.Null,
                because: "updatedAt must be null on creation");
            root.GetProperty("commentId").GetString().Should().Be(commentId.ToString(),
                because: "the reply must be linked to the correct parent comment");
        }

        // =========================================================
        // SC-2: POST reply without token → 401
        // =========================================================

        [Fact]
        public async Task CreateReply_NoToken_Returns401()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-SC2-{Guid.NewGuid():N}");
            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-SC2-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("sc2-user");
            Guid commentId = await userClient.ArrangeCreateCommentAsync(blogId, "Parent comment SC2.");

            // Act — anonymous client
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.CreateReplyAsync(commentId, "Anon reply.");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // =========================================================
        // SC-3: POST reply to non-existent commentId → 404
        // =========================================================

        [Fact]
        public async Task CreateReply_NonExistentCommentId_Returns404()
        {
            // Arrange
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("sc3-user");
            Guid fakeCommentId = Guid.NewGuid();

            // Act
            HttpResponseMessage response = await userClient.CreateReplyAsync(fakeCommentId, "Reply to ghost.");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // =========================================================
        // SC-4: POST reply with empty text → 400
        // =========================================================

        [Fact]
        public async Task CreateReply_EmptyText_Returns400()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-SC4-{Guid.NewGuid():N}");
            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-SC4-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("sc4-user");
            Guid commentId = await userClient.ArrangeCreateCommentAsync(blogId, "Parent SC4.");

            // Act
            HttpResponseMessage response = await userClient.CreateReplyAsync(commentId, string.Empty);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // =========================================================
        // SC-5: POST reply with text > 1000 chars → 400
        // =========================================================

        [Fact]
        public async Task CreateReply_TextExceeds1000Chars_Returns400()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-SC5-{Guid.NewGuid():N}");
            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-SC5-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("sc5-user");
            Guid commentId = await userClient.ArrangeCreateCommentAsync(blogId, "Parent SC5.");

            string tooLongText = new string('y', 1001);

            // Act
            HttpResponseMessage response = await userClient.CreateReplyAsync(commentId, tooLongText);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // =========================================================
        // SC-6: PUT own reply → 200, isEdited:true
        // =========================================================

        [Fact]
        public async Task UpdateReply_ByOwner_Returns200AndIsEditedTrue()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-SC6-{Guid.NewGuid():N}");
            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-SC6-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient ownerClient, _, _) = await _fixture.CreateUserClientAsync("sc6-owner");
            Guid commentId = await ownerClient.ArrangeCreateCommentAsync(blogId, "Parent SC6.");
            Guid replyId = await ownerClient.ArrangeCreateReplyAsync(commentId, "Original reply SC6.");

            // Act
            HttpResponseMessage response = await ownerClient.UpdateReplyAsync(
                commentId, replyId, "Updated reply SC6.");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.GetProperty("isEdited").GetBoolean().Should().BeTrue(
                because: "after update, isEdited must reflect that the reply was modified");
            root.GetProperty("subCommentText").GetString().Should().Be("Updated reply SC6.");
            root.GetProperty("updatedAt").ValueKind.Should().NotBe(JsonValueKind.Null,
                because: "updatedAt must be set after editing the reply");
        }

        // =========================================================
        // SC-7: PUT another user's reply (non-owner, non-admin) → 403
        // =========================================================

        [Fact]
        public async Task UpdateReply_ByNonOwner_Returns403()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-SC7-{Guid.NewGuid():N}");
            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-SC7-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient ownerClient, _, _) = await _fixture.CreateUserClientAsync("sc7-owner");
            Guid commentId = await ownerClient.ArrangeCreateCommentAsync(blogId, "Parent SC7.");
            Guid replyId = await ownerClient.ArrangeCreateReplyAsync(commentId, "Owner's reply SC7.");

            (HttpClient attackerClient, _, _) = await _fixture.CreateUserClientAsync("sc7-attacker");

            // Act
            HttpResponseMessage response = await attackerClient.UpdateReplyAsync(
                commentId, replyId, "Hacked reply.");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
                because: "only the reply owner may edit their reply");
        }

        // =========================================================
        // SC-8: DELETE own reply → 204
        // =========================================================

        [Fact]
        public async Task DeleteReply_ByOwner_Returns204()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-SC8-{Guid.NewGuid():N}");
            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-SC8-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient ownerClient, _, _) = await _fixture.CreateUserClientAsync("sc8-owner");
            Guid commentId = await ownerClient.ArrangeCreateCommentAsync(blogId, "Parent SC8.");
            Guid replyId = await ownerClient.ArrangeCreateReplyAsync(commentId, "Reply to delete SC8.");

            // Act
            HttpResponseMessage response = await ownerClient.DeleteReplyAsync(commentId, replyId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        // =========================================================
        // SC-9: DELETE reply by admin (not owner) → 204
        // =========================================================

        [Fact]
        public async Task DeleteReply_ByAdmin_Returns204()
        {
            // Arrange — user creates reply; admin deletes it
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-SC9-{Guid.NewGuid():N}");
            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-SC9-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("sc9-user");
            Guid commentId = await userClient.ArrangeCreateCommentAsync(blogId, "Parent SC9.");
            Guid replyId = await userClient.ArrangeCreateReplyAsync(commentId, "User reply to admin-delete.");

            // Act
            HttpResponseMessage response = await adminClient.DeleteReplyAsync(commentId, replyId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent,
                because: "admin must be able to delete any reply regardless of ownership");
        }

        // =========================================================
        // SC-10: DELETE reply by another normal user → 403
        // =========================================================

        [Fact]
        public async Task DeleteReply_ByNonOwnerNonAdmin_Returns403()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-SC10-{Guid.NewGuid():N}");
            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-SC10-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient ownerClient, _, _) = await _fixture.CreateUserClientAsync("sc10-owner");
            Guid commentId = await ownerClient.ArrangeCreateCommentAsync(blogId, "Parent SC10.");
            Guid replyId = await ownerClient.ArrangeCreateReplyAsync(commentId, "Owner's reply SC10.");

            (HttpClient attackerClient, _, _) = await _fixture.CreateUserClientAsync("sc10-attacker");

            // Act
            HttpResponseMessage response = await attackerClient.DeleteReplyAsync(commentId, replyId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
                because: "only the reply owner or admin may delete a reply");
        }

        // =========================================================
        // SC-11: Cascade — parent comment deleted → reply POST to orphaned commentId → 404
        // =========================================================

        [Fact]
        public async Task CreateReply_AfterParentCommentDeleted_Returns404()
        {
            // Arrange — create blog, comment, and a reply; then delete the parent comment
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-SC11-{Guid.NewGuid():N}");
            // Faz 5: blog creation requires Admin or Manager role.
            Guid blogId = await adminClient.ArrangeCreateBlogAsync($"Blog-SC11-{Guid.NewGuid():N}", "Desc", catId);
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("sc11-user");
            Guid commentId = await userClient.ArrangeCreateCommentAsync(blogId, "Parent SC11 — will be deleted.");
            await userClient.ArrangeCreateReplyAsync(commentId, "Existing reply SC11.");

            // Delete the parent comment (cascade removes sub-comments via DB)
            HttpResponseMessage deleteResponse = await userClient.DeleteCommentAsync(blogId, commentId);
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
                because: "setup — comment deletion must succeed");

            // Act — try to add a reply to the now-deleted comment
            (HttpClient otherUserClient, _, _) = await _fixture.CreateUserClientAsync("sc11-other");
            HttpResponseMessage replyResponse = await otherUserClient.CreateReplyAsync(commentId, "Orphan reply.");

            // Assert — parent comment no longer exists; 404 expected
            replyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound,
                because: "the parent comment was deleted; adding a reply to it must return 404");
        }
    }
}
