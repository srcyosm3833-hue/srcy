using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.Phase5
{
    /// <summary>
    /// Integration tests for the Blog Search endpoint (Faz 5 Feature 1).
    /// Endpoint: GET /api/blogs/search?q={term}&amp;page=&amp;pageSize=&amp;categoryId=
    ///
    /// Covered scenarios:
    ///   S-01. Title match → blog appears in results
    ///   S-02. Description match → blog appears in results
    ///   S-03. No match → 200 + empty items + totalCount=0
    ///   S-04. CategoryId filter → only that category's blogs returned
    ///   S-05. Soft-deleted blog → does NOT appear in search results
    ///   S-06. q empty/whitespace → 400
    ///   S-07. q longer than 200 characters → 400
    ///   S-08. Anonymous access → 200 (search is public, no auth required)
    ///   S-09. Paged result shape is correct (items, totalCount, page, pageSize, totalPages)
    /// </summary>
    [Collection(BlogApiCollection.CollectionName)]
    public sealed class BlogSearchTests
    {
        private readonly BlogApiFixture _fixture;

        public BlogSearchTests(BlogApiFixture fixture)
        {
            _fixture = fixture;
        }

        // =========================================================
        // S-01: Title match → blog appears in results
        // =========================================================

        [Fact]
        public async Task Search_TitleMatch_ReturnsBlog()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Search-Cat-{Guid.NewGuid():N}");

            // Use a unique term that won't collide with other tests' data
            string uniqueToken = Guid.NewGuid().ToString("N")[..12];
            string title = $"TitleSearch-{uniqueToken}";
            await adminClient.ArrangeCreateBlogAsync(title, "Some description content.", catId);

            // Act — search by a substring of the title
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.SearchBlogsAsync($"TitleSearch-{uniqueToken}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement items = doc.RootElement.GetProperty("items");
            int totalCount = doc.RootElement.GetProperty("totalCount").GetInt32();

            totalCount.Should().BeGreaterThan(0, because: "at least one blog matches the title term");
            bool found = false;
            foreach (JsonElement item in items.EnumerateArray())
            {
                if (item.GetProperty("title").GetString()?.Contains(uniqueToken) == true)
                {
                    found = true;
                }
            }
            found.Should().BeTrue($"blog with title containing '{uniqueToken}' must be in results");
        }

        // =========================================================
        // S-02: Description match → blog appears in results
        // =========================================================

        [Fact]
        public async Task Search_DescriptionMatch_ReturnsBlog()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Search-Desc-Cat-{Guid.NewGuid():N}");

            string uniqueToken = Guid.NewGuid().ToString("N")[..12];
            string description = $"Description contains the unique term desctoken{uniqueToken} here.";
            await adminClient.ArrangeCreateBlogAsync(
                $"NonMatchingTitle-{Guid.NewGuid():N}", description, catId);

            // Act — search for the unique description token
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.SearchBlogsAsync($"desctoken{uniqueToken}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            int totalCount = doc.RootElement.GetProperty("totalCount").GetInt32();
            totalCount.Should().BeGreaterThan(0,
                because: "the unique description token must match at least one blog");
        }

        // =========================================================
        // S-03: No match → 200 + empty items + totalCount=0
        // =========================================================

        [Fact]
        public async Task Search_NoMatch_Returns200WithEmptyItems()
        {
            // Arrange — a term that is extremely unlikely to match anything
            string neverMatchTerm = $"ZZZNOMATCH{Guid.NewGuid():N}ZZZNOMATCH";

            // Act
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.SearchBlogsAsync(neverMatchTerm);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.GetProperty("totalCount").GetInt32().Should().Be(0);
            root.GetProperty("items").GetArrayLength().Should().Be(0);
        }

        // =========================================================
        // S-04: CategoryId filter → only that category's blogs returned
        // =========================================================

        [Fact]
        public async Task Search_CategoryFilter_ReturnsOnlyThatCategorysBlogs()
        {
            // Arrange — two categories, a shared search term, one blog in each
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catA = await adminClient.ArrangeCreateCategoryAsync($"CatA-Filter-{Guid.NewGuid():N}");
            Guid catB = await adminClient.ArrangeCreateCategoryAsync($"CatB-Filter-{Guid.NewGuid():N}");

            string sharedToken = Guid.NewGuid().ToString("N")[..10];
            string titleA = $"SharedTerm{sharedToken} CatA Blog";
            string titleB = $"SharedTerm{sharedToken} CatB Blog";

            await adminClient.ArrangeCreateBlogAsync(titleA, "Description A.", catA);
            await adminClient.ArrangeCreateBlogAsync(titleB, "Description B.", catB);

            // Act — search with categoryId = catA
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.SearchBlogsAsync(
                $"SharedTerm{sharedToken}", categoryId: catA);

            // Assert — only catA's blog should be in results
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement items = doc.RootElement.GetProperty("items");

            bool catAFound = false;
            bool catBFound = false;
            foreach (JsonElement item in items.EnumerateArray())
            {
                string? catId = item.TryGetProperty("categoryId", out JsonElement cid)
                    ? cid.GetString()
                    : null;
                if (catId == catA.ToString()) catAFound = true;
                if (catId == catB.ToString()) catBFound = true;
            }

            catAFound.Should().BeTrue("catA blog must be returned for catA filter");
            catBFound.Should().BeFalse("catB blog must not appear when filtering by catA");
        }

        // =========================================================
        // S-05: Soft-deleted blog → does NOT appear in search results
        // =========================================================

        [Fact]
        public async Task Search_SoftDeletedBlog_DoesNotAppear()
        {
            // Arrange — create a blog, then soft-delete it via DELETE endpoint
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"SoftDel-Search-Cat-{Guid.NewGuid():N}");

            string uniqueToken = Guid.NewGuid().ToString("N")[..12];
            string title = $"SoftDelSearch{uniqueToken}";
            Guid blogId = await adminClient.ArrangeCreateBlogAsync(title, "Will be soft-deleted.", catId);

            // Verify it appears before deletion
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage beforeDelete = await anonClient.SearchBlogsAsync($"SoftDelSearch{uniqueToken}");
            beforeDelete.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument docBefore = await beforeDelete.ReadAsJsonDocumentAsync();
            docBefore.RootElement.GetProperty("totalCount").GetInt32().Should().BeGreaterThan(0,
                because: "blog must be visible before deletion");

            // Soft-delete the blog
            HttpResponseMessage deleteResp = await adminClient.DeleteBlogAsync(blogId);
            deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Act — search again
            HttpResponseMessage afterDelete = await anonClient.SearchBlogsAsync($"SoftDelSearch{uniqueToken}");

            // Assert
            afterDelete.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument docAfter = await afterDelete.ReadAsJsonDocumentAsync();
            docAfter.RootElement.GetProperty("totalCount").GetInt32().Should().Be(0,
                because: "soft-deleted blog must not appear in search results (global query filter)");
        }

        // =========================================================
        // S-06: q empty → 400 (FluentValidation: NotEmpty)
        // =========================================================

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Search_EmptyOrWhitespaceQuery_Returns400(string q)
        {
            // Arrange
            using HttpClient anonClient = _fixture.CreateClient();

            // Act — send raw request to bypass URI escaping of whitespace
            string url = $"/api/blogs/search?q={q}";
            HttpResponseMessage response = await anonClient.GetAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                because: $"empty/whitespace query '{q}' must be rejected by validator");
        }

        // =========================================================
        // S-07: q > 200 characters → 400 (FluentValidation: MaximumLength(200))
        // =========================================================

        [Fact]
        public async Task Search_QueryExceeds200Chars_Returns400()
        {
            // Arrange — 201 character query
            string longQuery = new string('a', 201);

            using HttpClient anonClient = _fixture.CreateClient();

            // Act
            HttpResponseMessage response = await anonClient.SearchBlogsAsync(longQuery);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                because: "query exceeding 200 characters must be rejected");
        }

        // =========================================================
        // S-08: Anonymous access → 200 (search is public)
        // =========================================================

        [Fact]
        public async Task Search_AnonymousUser_Returns200()
        {
            // Arrange — no auth header
            using HttpClient anonClient = _fixture.CreateClient();

            // Act
            HttpResponseMessage response = await anonClient.SearchBlogsAsync("anything");

            // Assert
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
                because: "search endpoint is public (AllowAnonymous)");
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
            // Either 200 (found or empty) is acceptable
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // =========================================================
        // S-09: Paged result shape is correct
        // =========================================================

        [Fact]
        public async Task Search_ValidQuery_ReturnsCorrectPagedShape()
        {
            // Arrange — create a blog with a unique token in its title
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid catId = await adminClient.ArrangeCreateCategoryAsync($"Shape-Search-Cat-{Guid.NewGuid():N}");
            string uniqueToken = Guid.NewGuid().ToString("N")[..12];
            await adminClient.ArrangeCreateBlogAsync($"ShapeTest{uniqueToken}", "A description.", catId);

            // Act
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.SearchBlogsAsync($"ShapeTest{uniqueToken}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.TryGetProperty("items", out _).Should().BeTrue();
            root.TryGetProperty("totalCount", out _).Should().BeTrue();
            root.TryGetProperty("page", out JsonElement pageProp).Should().BeTrue();
            root.TryGetProperty("pageSize", out _).Should().BeTrue();
            root.TryGetProperty("totalPages", out _).Should().BeTrue();
            pageProp.GetInt32().Should().Be(1, because: "default page is 1");
        }
    }
}
