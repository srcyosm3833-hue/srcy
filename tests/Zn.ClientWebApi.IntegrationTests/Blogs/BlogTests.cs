using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.Blogs
{
    /// <summary>
    /// Integration tests for the Blog endpoints.
    ///
    /// Covered scenarios (F2-T1 Blog scope):
    ///   10.  GET /api/blogs                 → 200, PagedResult shape correct
    ///   11a. GET /api/blogs pageSize clamp  → pageSize clamped to MaxPageSize(50)
    ///   11b. GET /api/blogs categoryId filter → only that category's blogs returned
    ///   12a. GET /api/blogs/{id} (valid)    → 200, all detail fields present
    ///   12b. GET /api/blogs/{id} (missing)  → 404
    ///   13a. POST /api/blogs authenticated  → 201, AuthorId matches token user
    ///   13b. POST /api/blogs no token       → 401
    ///   14.  POST /api/blogs non-existent categoryId → 400
    ///   15.  PUT /api/blogs/{id} by author  → 200
    ///   16.  PUT /api/blogs/{id} by other user (not author, not admin) → 403
    ///   17a. DELETE /api/blogs/{id} by other user → 403
    ///   17b. DELETE /api/blogs/{id} by admin (not author) → 204
    ///   18.  DELETE /api/blogs/{id} by author → 204, then GET → 404
    ///   19a. PUT non-existent blog          → 404 (before 403)
    ///   19b. DELETE non-existent blog       → 404 (before 403)
    /// </summary>
    [Collection(BlogApiCollection.CollectionName)]
    public sealed class BlogTests
    {
        private readonly BlogApiFixture _fixture;

        public BlogTests(BlogApiFixture fixture)
        {
            _fixture = fixture;
        }

        // =========================================================
        // Scenario 10: GET /api/blogs → 200, PagedResult shape
        // =========================================================

        [Fact]
        public async Task GetBlogs_Returns200WithCorrectPagedResultShape()
        {
            // Arrange — create category + blog so list is non-trivially populated
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Shape-{Guid.NewGuid():N}");

            (HttpClient authorClient, _, _) = await _fixture.CreateUserClientAsync("shape-author");
            await authorClient.ArrangeCreateBlogAsync($"Shape Blog {Guid.NewGuid():N}", "Desc", catId);

            // Act
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.GetBlogsAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            // PagedResult must expose all expected pagination fields
            root.TryGetProperty("items", out _).Should().BeTrue("items must be present");
            root.TryGetProperty("totalCount", out JsonElement totalCountProp).Should().BeTrue();
            root.TryGetProperty("page", out JsonElement pageProp).Should().BeTrue();
            root.TryGetProperty("pageSize", out JsonElement pageSizeProp).Should().BeTrue();
            root.TryGetProperty("totalPages", out _).Should().BeTrue("totalPages must be present");

            totalCountProp.GetInt32().Should().BeGreaterThan(0);
            pageProp.GetInt32().Should().Be(1);
            pageSizeProp.GetInt32().Should().Be(10);
        }

        // =========================================================
        // Scenario 11a: pageSize > MaxPageSize (50) → clamped to 50
        // =========================================================

        [Fact]
        public async Task GetBlogs_PageSizeExceedsMaximum_ClampedTo50()
        {
            // Act — request pageSize=200 which exceeds the 50 upper limit
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.GetBlogsAsync(page: 1, pageSize: 200);

            // Assert — still 200, but pageSize in response must be clamped to 50
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            int returnedPageSize = doc.RootElement.GetProperty("pageSize").GetInt32();
            returnedPageSize.Should().Be(50,
                because: "handler must clamp pageSize to MaxPageSize=50");
        }

        // =========================================================
        // Scenario 11b: categoryId filter → only that category's blogs
        // =========================================================

        [Fact]
        public async Task GetBlogs_CategoryIdFilter_ReturnsOnlyThatCategorysBlogs()
        {
            // Arrange — two categories, one blog each
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catA = await adminClient.ArrangeCreateCategoryAsync($"Cat-FilterA-{Guid.NewGuid():N}");
            Guid catB = await adminClient.ArrangeCreateCategoryAsync($"Cat-FilterB-{Guid.NewGuid():N}");

            (HttpClient authorClient, _, _) = await _fixture.CreateUserClientAsync("filter-author");
            string titleInA = $"BlogInA-{Guid.NewGuid():N}";
            string titleInB = $"BlogInB-{Guid.NewGuid():N}";
            await authorClient.ArrangeCreateBlogAsync(titleInA, "Desc A", catA);
            await authorClient.ArrangeCreateBlogAsync(titleInB, "Desc B", catB);

            // Act — filter by catA only
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.GetBlogsAsync(categoryId: catA);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement items = doc.RootElement.GetProperty("items");

            bool foundInA = false;
            foreach (JsonElement item in items.EnumerateArray())
            {
                // No item from catB must appear
                item.GetProperty("categoryId").GetString()
                    .Should().Be(catA.ToString(),
                        because: "filter by categoryId must exclude blogs from other categories");

                if (item.GetProperty("title").GetString() == titleInA)
                {
                    foundInA = true;
                }
            }

            foundInA.Should().BeTrue("blog from catA must be present when filtering by catA");
        }

        // =========================================================
        // Scenario 12a: GET /api/blogs/{id} valid → 200, all detail fields
        // =========================================================

        [Fact]
        public async Task GetBlogById_ExistingId_Returns200WithAllDetailFields()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Detail-{Guid.NewGuid():N}");

            (HttpClient authorClient, string authorUserId, _) = await _fixture.CreateUserClientAsync("detail-author");
            Guid blogId = await authorClient.ArrangeCreateBlogAsync($"Detail Blog {Guid.NewGuid():N}", "Detail desc", catId);

            // Act
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.GetBlogByIdAsync(blogId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            // All expected fields must be present
            root.TryGetProperty("id", out _).Should().BeTrue();
            root.TryGetProperty("title", out _).Should().BeTrue();
            root.TryGetProperty("coverImage", out _).Should().BeTrue();
            root.TryGetProperty("blogImage", out _).Should().BeTrue();
            root.TryGetProperty("description", out _).Should().BeTrue();
            root.TryGetProperty("categoryId", out _).Should().BeTrue();
            root.TryGetProperty("categoryName", out _).Should().BeTrue();
            root.TryGetProperty("authorId", out JsonElement authorIdProp).Should().BeTrue();
            root.TryGetProperty("authorName", out _).Should().BeTrue();
            root.TryGetProperty("createdAt", out _).Should().BeTrue();

            // AuthorId must match the user who created the blog
            authorIdProp.GetString().Should().Be(authorUserId,
                because: "authorId in the response must match the token user who created the blog");
        }

        // =========================================================
        // Scenario 12b: GET /api/blogs/{id} non-existent → 404
        // =========================================================

        [Fact]
        public async Task GetBlogById_NonExistentId_Returns404()
        {
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.GetBlogByIdAsync(Guid.NewGuid());

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // =========================================================
        // Scenario 13a: POST authenticated → 201, AuthorId = token user (body UserId ignored)
        // =========================================================

        [Fact]
        public async Task CreateBlog_AuthenticatedUser_Returns201WithTokenUserAsAuthor()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Create13a-{Guid.NewGuid():N}");

            (HttpClient authorClient, string authorUserId, _) = await _fixture.CreateUserClientAsync("create-author");

            // Act — body has no UserId field; controller must use token
            HttpResponseMessage response = await authorClient.CreateBlogAsync(
                $"Blog-{Guid.NewGuid():N}",
                "Some description",
                "https://example.com/cover.png",
                "https://example.com/blog.png",
                catId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.TryGetProperty("id", out _).Should().BeTrue();
            root.GetProperty("authorId").GetString()
                .Should().Be(authorUserId,
                    because: "AuthorId must be taken from the access token, not the request body");
        }

        // =========================================================
        // Scenario 13b: POST without token → 401
        // =========================================================

        [Fact]
        public async Task CreateBlog_NoToken_Returns401()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Create13b-{Guid.NewGuid():N}");

            using HttpClient anonClient = _fixture.CreateClient();

            // Act
            HttpResponseMessage response = await anonClient.CreateBlogAsync(
                $"Blog-{Guid.NewGuid():N}", "Desc",
                "https://example.com/cover.png",
                "https://example.com/blog.png",
                catId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // =========================================================
        // Scenario 14: POST with non-existent categoryId → 400
        // =========================================================

        [Fact]
        public async Task CreateBlog_NonExistentCategoryId_Returns400()
        {
            // Arrange — a random Guid that was never inserted as a category
            (HttpClient authorClient, _, _) = await _fixture.CreateUserClientAsync("create-badcat");
            Guid fakeCatId = Guid.NewGuid();

            // Act
            HttpResponseMessage response = await authorClient.CreateBlogAsync(
                $"Blog-{Guid.NewGuid():N}", "Desc",
                "https://example.com/cover.png",
                "https://example.com/blog.png",
                fakeCatId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // =========================================================
        // Scenario 15: PUT by the author → 200
        // =========================================================

        [Fact]
        public async Task UpdateBlog_ByAuthor_Returns200WithUpdatedData()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Update15-{Guid.NewGuid():N}");

            (HttpClient authorClient, _, _) = await _fixture.CreateUserClientAsync("update-author");
            Guid blogId = await authorClient.ArrangeCreateBlogAsync($"Blog-Orig-{Guid.NewGuid():N}", "Orig desc", catId);

            string updatedTitle = $"Updated-{Guid.NewGuid():N}";

            // Act
            HttpResponseMessage response = await authorClient.UpdateBlogAsync(
                blogId,
                updatedTitle,
                "Updated description",
                "https://example.com/cover2.png",
                "https://example.com/blog2.png",
                catId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            doc.RootElement.GetProperty("title").GetString().Should().Be(updatedTitle);
        }

        // =========================================================
        // Scenario 16 (CRITICAL): PUT by a different user (not author, not admin) → 403
        // =========================================================

        [Fact]
        public async Task UpdateBlog_ByNonAuthorNonAdmin_Returns403()
        {
            // Arrange — author creates blog; attacker tries to update it
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Update16-{Guid.NewGuid():N}");

            (HttpClient authorClient, _, _) = await _fixture.CreateUserClientAsync("author-16");
            Guid blogId = await authorClient.ArrangeCreateBlogAsync($"Blog-Auth-{Guid.NewGuid():N}", "Desc", catId);

            (HttpClient attackerClient, _, _) = await _fixture.CreateUserClientAsync("attacker-16");

            // Act — attacker tries to update author's blog
            HttpResponseMessage response = await attackerClient.UpdateBlogAsync(
                blogId,
                $"Hacked-{Guid.NewGuid():N}",
                "Hacked description",
                "https://example.com/cover-hacked.png",
                "https://example.com/blog-hacked.png",
                catId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
                because: "only the author or an admin may update a blog");
        }

        // =========================================================
        // Scenario 17a (CRITICAL): DELETE by a different user → 403
        // =========================================================

        [Fact]
        public async Task DeleteBlog_ByNonAuthorNonAdmin_Returns403()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Delete17a-{Guid.NewGuid():N}");

            (HttpClient authorClient, _, _) = await _fixture.CreateUserClientAsync("author-17a");
            Guid blogId = await authorClient.ArrangeCreateBlogAsync($"Blog-Del17a-{Guid.NewGuid():N}", "Desc", catId);

            (HttpClient attackerClient, _, _) = await _fixture.CreateUserClientAsync("attacker-17a");

            // Act
            HttpResponseMessage response = await attackerClient.DeleteBlogAsync(blogId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
                because: "a non-author non-admin must not be allowed to delete another user's blog");
        }

        // =========================================================
        // Scenario 17b (CRITICAL): DELETE by admin (not author) → 204
        // =========================================================

        [Fact]
        public async Task DeleteBlog_ByAdminNotAuthor_Returns204()
        {
            // Arrange — regular user creates blog; admin deletes it
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Delete17b-{Guid.NewGuid():N}");

            (HttpClient authorClient, _, _) = await _fixture.CreateUserClientAsync("author-17b");
            Guid blogId = await authorClient.ArrangeCreateBlogAsync($"Blog-Del17b-{Guid.NewGuid():N}", "Desc", catId);

            // Act — admin deletes a blog they did not author
            HttpResponseMessage response = await adminClient.DeleteBlogAsync(blogId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent,
                because: "admin must be able to delete any blog regardless of authorship");
        }

        // =========================================================
        // Scenario 18: DELETE by author → 204; subsequent GET → 404
        // =========================================================

        [Fact]
        public async Task DeleteBlog_ByAuthor_Returns204ThenGetReturns404()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Delete18-{Guid.NewGuid():N}");

            (HttpClient authorClient, _, _) = await _fixture.CreateUserClientAsync("author-18");
            Guid blogId = await authorClient.ArrangeCreateBlogAsync($"Blog-Del18-{Guid.NewGuid():N}", "Desc", catId);

            // Act
            HttpResponseMessage deleteResponse = await authorClient.DeleteBlogAsync(blogId);

            // Assert — delete returns 204
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify — GET the deleted blog must return 404
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage getResponse = await anonClient.GetBlogByIdAsync(blogId);
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound,
                because: "deleted blog must no longer be retrievable");
        }

        // =========================================================
        // Scenario 19a: PUT non-existent blog by authenticated user → 404 (not 403)
        // =========================================================

        [Fact]
        public async Task UpdateBlog_NonExistentId_Returns404NotForbidden()
        {
            // Arrange — an authenticated (non-admin) user making the request
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Cat-404PUT-{Guid.NewGuid():N}");

            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("user-404put");
            Guid ghostId = Guid.NewGuid();

            // Act
            HttpResponseMessage response = await userClient.UpdateBlogAsync(
                ghostId,
                "Any title",
                "Any desc",
                "https://example.com/cover.png",
                "https://example.com/blog.png",
                catId);

            // Assert — 404 must take priority over 403 (record existence must not be leaked)
            response.StatusCode.Should().Be(HttpStatusCode.NotFound,
                because: "a non-existent blog must return 404 regardless of who is asking — resource existence must not leak as 403");
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden,
                because: "returning 403 for a non-existent resource would reveal its absence");
        }

        // =========================================================
        // Scenario 19b: DELETE non-existent blog by authenticated user → 404 (not 403)
        // =========================================================

        [Fact]
        public async Task DeleteBlog_NonExistentId_Returns404NotForbidden()
        {
            // Arrange
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("user-404del");
            Guid ghostId = Guid.NewGuid();

            // Act
            HttpResponseMessage response = await userClient.DeleteBlogAsync(ghostId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound,
                because: "a non-existent blog must return 404 regardless of who is asking");
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }
    }
}
