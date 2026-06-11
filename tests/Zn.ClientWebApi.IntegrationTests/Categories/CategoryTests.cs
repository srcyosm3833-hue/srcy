using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.Categories
{
    /// <summary>
    /// Integration tests for the Category endpoints.
    ///
    /// Covered scenarios (F2-T1 Category scope):
    ///   1.  GET /api/categories               → 200, list with BlogCount field
    ///   2a. GET /api/categories/{id} (valid)  → 200
    ///   2b. GET /api/categories/{id} (missing)→ 404
    ///   3.  POST /api/admin/categories admin  → 201
    ///   4a. POST without token                → 401
    ///   4b. POST with non-admin token         → 403
    ///   5.  POST duplicate name               → 409
    ///   6.  POST empty CategoryName           → 400
    ///   7a. PUT /api/admin/categories/{id}    → 200
    ///   7b. PUT missing id                    → 404
    ///   7c. PUT name collision with another   → 409
    ///   8a. DELETE existing (no blogs)        → 204
    ///   8b. DELETE missing id                 → 404
    ///   9.  DELETE category that has blogs    → 409
    /// </summary>
    [Collection(BlogApiCollection.CollectionName)]
    public sealed class CategoryTests
    {
        private readonly BlogApiFixture _fixture;

        public CategoryTests(BlogApiFixture fixture)
        {
            _fixture = fixture;
        }

        // =========================================================
        // Scenario 1: GET /api/categories → 200, items have BlogCount
        // =========================================================

        [Fact]
        public async Task GetAll_Returns200WithBlogCountField()
        {
            // Arrange — ensure at least one category exists
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string catName = $"Cat-GetAll-{Guid.NewGuid():N}";
            await adminClient.ArrangeCreateCategoryAsync(catName);

            // Act — anonymous request (public endpoint)
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.GetCategoriesAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            // Response must be a JSON array
            root.ValueKind.Should().Be(JsonValueKind.Array);

            // Find the category we just created
            bool found = false;
            foreach (JsonElement item in root.EnumerateArray())
            {
                if (item.GetProperty("categoryName").GetString() == catName)
                {
                    found = true;
                    // BlogCount field must exist and be ≥ 0
                    item.TryGetProperty("blogCount", out JsonElement blogCountProp)
                        .Should().BeTrue("every category response must include blogCount");
                    blogCountProp.GetInt32().Should().BeGreaterThanOrEqualTo(0);
                }
            }

            found.Should().BeTrue($"category '{catName}' should appear in the list");
        }

        // =========================================================
        // Scenario 2a: GET /api/categories/{id} (valid) → 200
        // =========================================================

        [Fact]
        public async Task GetById_ExistingId_Returns200()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string catName = $"Cat-GetById-{Guid.NewGuid():N}";
            Guid categoryId = await adminClient.ArrangeCreateCategoryAsync(catName);

            // Act
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.GetCategoryByIdAsync(categoryId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            doc.RootElement.GetProperty("id").GetString()
                .Should().Be(categoryId.ToString());
            doc.RootElement.GetProperty("categoryName").GetString()
                .Should().Be(catName);
        }

        // =========================================================
        // Scenario 2b: GET /api/categories/{id} (non-existent) → 404
        // =========================================================

        [Fact]
        public async Task GetById_NonExistentId_Returns404()
        {
            // Arrange — a random Guid that was never inserted
            Guid ghostId = Guid.NewGuid();

            // Act
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.GetCategoryByIdAsync(ghostId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // =========================================================
        // Scenario 3: POST admin → 201 with created category
        // =========================================================

        [Fact]
        public async Task Create_AdminToken_Returns201WithCategoryData()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string catName = $"Cat-Create-{Guid.NewGuid():N}";

            // Act
            HttpResponseMessage response = await adminClient.CreateCategoryAsync(catName);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;
            root.GetProperty("categoryName").GetString().Should().Be(catName);
            root.TryGetProperty("id", out JsonElement idProp).Should().BeTrue();
            idProp.GetString().Should().NotBeNullOrWhiteSpace();
            root.GetProperty("blogCount").GetInt32().Should().Be(0,
                because: "newly created category has no blogs");
        }

        // =========================================================
        // Scenario 4a: POST without token → 401
        // =========================================================

        [Fact]
        public async Task Create_NoToken_Returns401()
        {
            // Arrange — anonymous client (no auth header)
            using HttpClient anonClient = _fixture.CreateClient();

            // Act
            HttpResponseMessage response = await anonClient.CreateCategoryAsync($"Cat-Anon-{Guid.NewGuid():N}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // =========================================================
        // Scenario 4b: POST with non-admin (regular user) token → 403
        // =========================================================

        [Fact]
        public async Task Create_NonAdminToken_Returns403()
        {
            // Arrange — register + login a regular (non-admin) user
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("cat-forbidden");

            // Act
            HttpResponseMessage response = await userClient.CreateCategoryAsync($"Cat-Forbidden-{Guid.NewGuid():N}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // =========================================================
        // Scenario 5: POST duplicate name → 409
        // =========================================================

        [Fact]
        public async Task Create_DuplicateName_Returns409()
        {
            // Arrange — create first category
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string catName = $"Cat-Dup-{Guid.NewGuid():N}";
            HttpResponseMessage first = await adminClient.CreateCategoryAsync(catName);
            first.StatusCode.Should().Be(HttpStatusCode.Created, "first creation must succeed");

            // Act — create second category with same name
            HttpResponseMessage response = await adminClient.CreateCategoryAsync(catName);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        // =========================================================
        // Scenario 6: POST empty CategoryName → 400
        // =========================================================

        [Fact]
        public async Task Create_EmptyCategoryName_Returns400()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();

            // Act
            HttpResponseMessage response = await adminClient.CreateCategoryAsync("");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // =========================================================
        // Scenario 7a: PUT existing category → 200
        // =========================================================

        [Fact]
        public async Task Update_ExistingCategory_Returns200WithNewName()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid categoryId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Upd-Old-{Guid.NewGuid():N}");
            string newName = $"Cat-Upd-New-{Guid.NewGuid():N}";

            // Act
            HttpResponseMessage response = await adminClient.UpdateCategoryAsync(categoryId, newName);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            doc.RootElement.GetProperty("categoryName").GetString().Should().Be(newName);
        }

        // =========================================================
        // Scenario 7b: PUT non-existent id → 404
        // =========================================================

        [Fact]
        public async Task Update_NonExistentId_Returns404()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid ghostId = Guid.NewGuid();

            // Act
            HttpResponseMessage response = await adminClient.UpdateCategoryAsync(ghostId, $"AnyName-{Guid.NewGuid():N}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // =========================================================
        // Scenario 7c: PUT name that collides with another category → 409
        // =========================================================

        [Fact]
        public async Task Update_NameCollisionWithOtherCategory_Returns409()
        {
            // Arrange — create two distinct categories
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string nameA = $"Cat-Coll-A-{Guid.NewGuid():N}";
            string nameB = $"Cat-Coll-B-{Guid.NewGuid():N}";

            Guid idA = await adminClient.ArrangeCreateCategoryAsync(nameA);
            await adminClient.ArrangeCreateCategoryAsync(nameB);

            // Act — try to rename A to B (conflict)
            HttpResponseMessage response = await adminClient.UpdateCategoryAsync(idA, nameB);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        // =========================================================
        // Scenario 8a: DELETE existing category (no blogs) → 204
        // =========================================================

        [Fact]
        public async Task Delete_ExistingCategoryWithNoBlogs_Returns204()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid categoryId = await adminClient.ArrangeCreateCategoryAsync($"Cat-Del-{Guid.NewGuid():N}");

            // Act
            HttpResponseMessage response = await adminClient.DeleteCategoryAsync(categoryId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify: subsequent GET must return 404
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage getResponse = await anonClient.GetCategoryByIdAsync(categoryId);
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound,
                because: "deleted category must no longer be found");
        }

        // =========================================================
        // Scenario 8b: DELETE non-existent id → 404
        // =========================================================

        [Fact]
        public async Task Delete_NonExistentId_Returns404()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid ghostId = Guid.NewGuid();

            // Act
            HttpResponseMessage response = await adminClient.DeleteCategoryAsync(ghostId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // =========================================================
        // Scenario 9: DELETE category with assigned blogs → 409
        // =========================================================

        [Fact]
        public async Task Delete_CategoryWithBlogs_Returns409()
        {
            // Arrange — create a category, then create a blog in that category
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid categoryId = await adminClient.ArrangeCreateCategoryAsync($"Cat-HasBlogs-{Guid.NewGuid():N}");

            // Use a regular user to create a blog (any authenticated user can create blogs)
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("cat-hasblogs-author");
            await userClient.ArrangeCreateBlogAsync(
                $"Blog in category {Guid.NewGuid():N}",
                "Some description for the blog.",
                categoryId);

            // Act — admin tries to delete the category that still has a blog
            HttpResponseMessage response = await adminClient.DeleteCategoryAsync(categoryId);

            // Assert — FK Restrict → handler returns 409 before hitting the DB constraint
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
    }
}
