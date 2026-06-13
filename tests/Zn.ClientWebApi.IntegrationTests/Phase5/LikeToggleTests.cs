using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.Phase5
{
    /// <summary>
    /// Integration tests for the Blog Like and Comment Like toggle endpoints (Faz 5 Feature A9/A10).
    ///
    /// Blog Like endpoint : POST /api/blogs/{id}/like   → [Authorize]
    /// Comment Like endpoint: POST /api/comments/{id}/like → [Authorize]
    ///
    /// Both return: { liked: bool, likeCount: int }
    ///
    /// Covered scenarios — Blog Like:
    ///   BL-01. Authenticated user toggles like on a blog → 200, liked=true, likeCount=1
    ///   BL-02. Anonymous user attempts to like a blog → 401 Unauthorized
    ///   BL-03. Idempotent toggle: second call removes like (liked=false, likeCount=0)
    ///   BL-04. Idempotent toggle: third call re-adds like (liked=true, likeCount=1)
    ///   BL-05. Multiple distinct users like the same blog → likeCount equals the number of users
    ///   BL-06. Like on a non-existent blog → 404 Not Found
    ///
    /// Covered scenarios — Comment Like:
    ///   CL-01. Authenticated user toggles like on a comment → 200, liked=true, likeCount=1
    ///   CL-02. Anonymous user attempts to like a comment → 401 Unauthorized
    ///   CL-03. Idempotent toggle: second call removes like (liked=false, likeCount=0)
    ///   CL-04. Idempotent toggle: third call re-adds like (liked=true, likeCount=1)
    ///   CL-05. Multiple distinct users like the same comment → likeCount equals the number of users
    ///   CL-06. Like on a non-existent comment → 404 Not Found
    /// </summary>
    [Collection(BlogApiCollection.CollectionName)]
    public sealed class LikeToggleTests
    {
        private readonly BlogApiFixture _fixture;

        public LikeToggleTests(BlogApiFixture fixture)
        {
            _fixture = fixture;
        }

        // =========================================================
        // Shared arrange: creates a category + blog owned by admin.
        // =========================================================

        private async Task<(Guid BlogId, Guid CategoryId)> ArrangeBlogAsync()
        {
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync(
                $"LikeCat-{Guid.NewGuid():N}");
            Guid blogId = await adminClient.ArrangeCreateBlogAsync(
                $"LikeBlog-{Guid.NewGuid():N}",
                "A blog post for like tests.",
                catId);
            return (blogId, catId);
        }

        // =========================================================
        // BL-01: Authenticated user likes a blog → 200, liked=true, likeCount=1
        // =========================================================

        [Fact]
        public async Task ToggleBlogLike_AuthenticatedUser_ReturnsLikedTrueAndCountOne()
        {
            // Arrange
            (Guid blogId, _) = await ArrangeBlogAsync();
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("bl01");

            // Act
            HttpResponseMessage response = await userClient.ToggleBlogLikeAsync(blogId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                because: "an authenticated user must be able to like a blog");

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.GetProperty("liked").GetBoolean().Should().BeTrue(
                because: "first toggle on a blog adds the like");
            root.GetProperty("likeCount").GetInt32().Should().Be(1,
                because: "one user liked the blog");

            userClient.Dispose();
        }

        // =========================================================
        // BL-02: Anonymous user attempts to like a blog → 401
        // =========================================================

        [Fact]
        public async Task ToggleBlogLike_AnonymousUser_Returns401()
        {
            // Arrange
            (Guid blogId, _) = await ArrangeBlogAsync();
            using HttpClient anonClient = _fixture.CreateClient();

            // Act
            HttpResponseMessage response = await anonClient.ToggleBlogLikeAsync(blogId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                because: "unauthenticated requests to the like endpoint must be rejected");
        }

        // =========================================================
        // BL-03: Idempotent toggle — second call removes like
        // =========================================================

        [Fact]
        public async Task ToggleBlogLike_SecondCallByTheSameUser_RemovesLike()
        {
            // Arrange
            (Guid blogId, _) = await ArrangeBlogAsync();
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("bl03");

            // First call — adds like
            HttpResponseMessage firstResponse = await userClient.ToggleBlogLikeAsync(blogId);
            firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument firstDoc = await firstResponse.ReadAsJsonDocumentAsync();
            firstDoc.RootElement.GetProperty("liked").GetBoolean().Should().BeTrue();

            // Act — second call should remove the like
            HttpResponseMessage secondResponse = await userClient.ToggleBlogLikeAsync(blogId);

            // Assert
            secondResponse.StatusCode.Should().Be(HttpStatusCode.OK,
                because: "toggle is idempotent and always returns 200");

            JsonDocument secondDoc = await secondResponse.ReadAsJsonDocumentAsync();
            JsonElement root = secondDoc.RootElement;

            root.GetProperty("liked").GetBoolean().Should().BeFalse(
                because: "second toggle on the same blog removes the like");
            root.GetProperty("likeCount").GetInt32().Should().Be(0,
                because: "no users have an active like on the blog after unliking");

            userClient.Dispose();
        }

        // =========================================================
        // BL-04: Idempotent toggle — third call re-adds like
        // =========================================================

        [Fact]
        public async Task ToggleBlogLike_ThirdCallByTheSameUser_ReAddsLike()
        {
            // Arrange
            (Guid blogId, _) = await ArrangeBlogAsync();
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("bl04");

            // First call — adds like
            await userClient.ToggleBlogLikeAsync(blogId);

            // Second call — removes like
            await userClient.ToggleBlogLikeAsync(blogId);

            // Act — third call should add again
            HttpResponseMessage thirdResponse = await userClient.ToggleBlogLikeAsync(blogId);

            // Assert
            thirdResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument doc = await thirdResponse.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.GetProperty("liked").GetBoolean().Should().BeTrue(
                because: "third toggle re-adds the like");
            root.GetProperty("likeCount").GetInt32().Should().Be(1,
                because: "one user has an active like on the blog after the third toggle");

            userClient.Dispose();
        }

        // =========================================================
        // BL-05: Multiple distinct users like the same blog → correct count
        // =========================================================

        [Fact]
        public async Task ToggleBlogLike_TwoDistinctUsers_LikeCountIsTwo()
        {
            // Arrange
            (Guid blogId, _) = await ArrangeBlogAsync();
            (HttpClient user1Client, _, _) = await _fixture.CreateUserClientAsync("bl05a");
            (HttpClient user2Client, _, _) = await _fixture.CreateUserClientAsync("bl05b");

            // Act — both users like the blog
            HttpResponseMessage response1 = await user1Client.ToggleBlogLikeAsync(blogId);
            HttpResponseMessage response2 = await user2Client.ToggleBlogLikeAsync(blogId);

            // Assert — each user sees their own liked=true; the last response carries count=2
            response1.StatusCode.Should().Be(HttpStatusCode.OK);
            response2.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument doc1 = await response1.ReadAsJsonDocumentAsync();
            doc1.RootElement.GetProperty("liked").GetBoolean().Should().BeTrue(
                because: "first user's like must be recorded");

            JsonDocument doc2 = await response2.ReadAsJsonDocumentAsync();
            doc2.RootElement.GetProperty("liked").GetBoolean().Should().BeTrue(
                because: "second user's like must be recorded");
            doc2.RootElement.GetProperty("likeCount").GetInt32().Should().Be(2,
                because: "two distinct users liked the blog");

            user1Client.Dispose();
            user2Client.Dispose();
        }

        // =========================================================
        // BL-06: Like on a non-existent blog → 404
        // =========================================================

        [Fact]
        public async Task ToggleBlogLike_NonExistentBlog_Returns404()
        {
            // Arrange — a GUID that does not correspond to any blog
            Guid nonExistentBlogId = Guid.NewGuid();
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("bl06");

            // Act
            HttpResponseMessage response = await userClient.ToggleBlogLikeAsync(nonExistentBlogId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound,
                because: "liking a blog that does not exist must return 404");

            userClient.Dispose();
        }

        // =========================================================
        // CL-01: Authenticated user likes a comment → 200, liked=true, likeCount=1
        // =========================================================

        [Fact]
        public async Task ToggleCommentLike_AuthenticatedUser_ReturnsLikedTrueAndCountOne()
        {
            // Arrange
            (Guid blogId, _) = await ArrangeBlogAsync();
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("cl01");
            Guid commentId = await userClient.ArrangeCreateCommentAsync(blogId, "A comment for like test.");

            // Act
            HttpResponseMessage response = await userClient.ToggleCommentLikeAsync(commentId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                because: "an authenticated user must be able to like a comment");

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.GetProperty("liked").GetBoolean().Should().BeTrue(
                because: "first toggle on a comment adds the like");
            root.GetProperty("likeCount").GetInt32().Should().Be(1,
                because: "one user liked the comment");

            userClient.Dispose();
        }

        // =========================================================
        // CL-02: Anonymous user attempts to like a comment → 401
        // =========================================================

        [Fact]
        public async Task ToggleCommentLike_AnonymousUser_Returns401()
        {
            // Arrange
            (Guid blogId, _) = await ArrangeBlogAsync();
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("cl02-author");
            Guid commentId = await userClient.ArrangeCreateCommentAsync(blogId);
            userClient.Dispose();

            using HttpClient anonClient = _fixture.CreateClient();

            // Act
            HttpResponseMessage response = await anonClient.ToggleCommentLikeAsync(commentId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                because: "unauthenticated requests to the comment like endpoint must be rejected");
        }

        // =========================================================
        // CL-03: Idempotent toggle — second call removes comment like
        // =========================================================

        [Fact]
        public async Task ToggleCommentLike_SecondCallByTheSameUser_RemovesLike()
        {
            // Arrange
            (Guid blogId, _) = await ArrangeBlogAsync();
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("cl03");
            Guid commentId = await userClient.ArrangeCreateCommentAsync(blogId, "Comment for toggle test.");

            // First call — adds like
            HttpResponseMessage firstResponse = await userClient.ToggleCommentLikeAsync(commentId);
            firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument firstDoc = await firstResponse.ReadAsJsonDocumentAsync();
            firstDoc.RootElement.GetProperty("liked").GetBoolean().Should().BeTrue();

            // Act — second call should remove the like
            HttpResponseMessage secondResponse = await userClient.ToggleCommentLikeAsync(commentId);

            // Assert
            secondResponse.StatusCode.Should().Be(HttpStatusCode.OK,
                because: "toggle is idempotent and always returns 200");

            JsonDocument secondDoc = await secondResponse.ReadAsJsonDocumentAsync();
            JsonElement root = secondDoc.RootElement;

            root.GetProperty("liked").GetBoolean().Should().BeFalse(
                because: "second toggle on the same comment removes the like");
            root.GetProperty("likeCount").GetInt32().Should().Be(0,
                because: "no users have an active like on the comment after unliking");

            userClient.Dispose();
        }

        // =========================================================
        // CL-04: Idempotent toggle — third call re-adds comment like
        // =========================================================

        [Fact]
        public async Task ToggleCommentLike_ThirdCallByTheSameUser_ReAddsLike()
        {
            // Arrange
            (Guid blogId, _) = await ArrangeBlogAsync();
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("cl04");
            Guid commentId = await userClient.ArrangeCreateCommentAsync(blogId, "Comment for three toggles.");

            // First call — adds like
            await userClient.ToggleCommentLikeAsync(commentId);

            // Second call — removes like
            await userClient.ToggleCommentLikeAsync(commentId);

            // Act — third call should add again
            HttpResponseMessage thirdResponse = await userClient.ToggleCommentLikeAsync(commentId);

            // Assert
            thirdResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument doc = await thirdResponse.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.GetProperty("liked").GetBoolean().Should().BeTrue(
                because: "third toggle re-adds the like on the comment");
            root.GetProperty("likeCount").GetInt32().Should().Be(1,
                because: "one user has an active like on the comment after the third toggle");

            userClient.Dispose();
        }

        // =========================================================
        // CL-05: Multiple distinct users like the same comment → correct count
        // =========================================================

        [Fact]
        public async Task ToggleCommentLike_TwoDistinctUsers_LikeCountIsTwo()
        {
            // Arrange
            (Guid blogId, _) = await ArrangeBlogAsync();
            (HttpClient authorClient, _, _) = await _fixture.CreateUserClientAsync("cl05-author");
            Guid commentId = await authorClient.ArrangeCreateCommentAsync(blogId, "Shared comment for multi-user like.");

            (HttpClient user1Client, _, _) = await _fixture.CreateUserClientAsync("cl05a");
            (HttpClient user2Client, _, _) = await _fixture.CreateUserClientAsync("cl05b");

            // Act — both users like the comment
            HttpResponseMessage response1 = await user1Client.ToggleCommentLikeAsync(commentId);
            HttpResponseMessage response2 = await user2Client.ToggleCommentLikeAsync(commentId);

            // Assert
            response1.StatusCode.Should().Be(HttpStatusCode.OK);
            response2.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument doc1 = await response1.ReadAsJsonDocumentAsync();
            doc1.RootElement.GetProperty("liked").GetBoolean().Should().BeTrue(
                because: "first user's comment like must be recorded");

            JsonDocument doc2 = await response2.ReadAsJsonDocumentAsync();
            doc2.RootElement.GetProperty("liked").GetBoolean().Should().BeTrue(
                because: "second user's comment like must be recorded");
            doc2.RootElement.GetProperty("likeCount").GetInt32().Should().Be(2,
                because: "two distinct users liked the comment");

            authorClient.Dispose();
            user1Client.Dispose();
            user2Client.Dispose();
        }

        // =========================================================
        // CL-06: Like on a non-existent comment → 404
        // =========================================================

        [Fact]
        public async Task ToggleCommentLike_NonExistentComment_Returns404()
        {
            // Arrange — a GUID that does not correspond to any comment
            Guid nonExistentCommentId = Guid.NewGuid();
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("cl06");

            // Act
            HttpResponseMessage response = await userClient.ToggleCommentLikeAsync(nonExistentCommentId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound,
                because: "liking a comment that does not exist must return 404");

            userClient.Dispose();
        }
    }
}
