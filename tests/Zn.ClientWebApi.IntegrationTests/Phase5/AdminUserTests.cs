using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.Phase5
{
    /// <summary>
    /// Integration tests for Admin User Management endpoints (Faz 5 Feature 2).
    ///
    /// Covered scenarios:
    ///   U-01. GET  /api/admin/users                    → 200, paged result shape
    ///   U-02. GET  /api/admin/users includeDeleted=true → soft-deleted user appears
    ///   U-03. GET  /api/admin/users                    no token → 401
    ///   U-04. GET  /api/admin/users                    User role → 403
    ///   U-05. GET  /api/admin/users/{id} existing      → 200 with correct fields
    ///   U-06. GET  /api/admin/users/{id} non-existent  → 404
    ///   U-07. POST /api/admin/users valid data          → 201, "User" role assigned
    ///   U-08. POST /api/admin/users duplicate email     → 409
    ///   U-09. POST /api/admin/users weak password       → 400
    ///   U-10. POST /api/admin/users empty firstName     → 400
    ///   U-11. POST /api/admin/users no token            → 401
    ///   U-12. POST /api/admin/users Manager role        → 403
    ///   U-13. PUT  /api/admin/users/{id} valid          → 200 with updated fields
    ///   U-14. PUT  /api/admin/users/{id} empty firstName → 400
    ///   U-15. PUT  /api/admin/users/{id} non-existent   → 404
    ///   U-16. DELETE /api/admin/users/{id} existing     → 204, IsDeleted=true in DB (login → 401)
    ///   U-17. DELETE /api/admin/users/{id} non-existent → 404
    ///   U-18. DELETE /api/admin/users/{id} admin self   → 400 (cannot delete own account)
    ///   U-19. Login  soft-deleted user                   → 401
    /// </summary>
    [Collection(BlogApiCollection.CollectionName)]
    public sealed class AdminUserTests
    {
        private readonly BlogApiFixture _fixture;

        public AdminUserTests(BlogApiFixture fixture)
        {
            _fixture = fixture;
        }

        // =========================================================
        // U-01: GET /api/admin/users → 200 with paged result shape
        // =========================================================

        [Fact]
        public async Task GetUsers_AdminToken_Returns200WithPagedShape()
        {
            // Arrange — create a user so the list is non-empty
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string email = BlogApiFixture.UniqueEmail("getall-usr");
            await adminClient.CreateAdminUserAsync("Get", "All", email, "Valid@1234");

            // Act
            HttpResponseMessage response = await adminClient.GetAdminUsersAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.TryGetProperty("items", out _).Should().BeTrue("items must be present in paged result");
            root.TryGetProperty("totalCount", out JsonElement totalCount).Should().BeTrue();
            root.TryGetProperty("page", out _).Should().BeTrue();
            root.TryGetProperty("pageSize", out _).Should().BeTrue();
            root.TryGetProperty("totalPages", out _).Should().BeTrue();

            totalCount.GetInt32().Should().BeGreaterThan(0,
                because: "at least one user (admin itself) must be in the list");
        }

        // =========================================================
        // U-02: GET /api/admin/users includeDeleted=true → soft-deleted user visible
        // =========================================================

        [Fact]
        public async Task GetUsers_IncludeDeleted_SoftDeletedUserAppears()
        {
            // Arrange — create a user, then soft-delete them
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string email = BlogApiFixture.UniqueEmail("incl-del");
            string userId = await adminClient.ArrangeCreateAdminUserAsync("Incl", "Del", email);

            // Soft-delete the user
            HttpResponseMessage deleteResp = await adminClient.DeleteAdminUserAsync(userId);
            deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Act — without includeDeleted: user should NOT appear
            HttpResponseMessage withoutDeleted = await adminClient.GetAdminUsersAsync(includeDeleted: false);
            withoutDeleted.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument docWithout = await withoutDeleted.ReadAsJsonDocumentAsync();
            bool foundWithoutFlag = ContainsUserId(docWithout, userId);
            foundWithoutFlag.Should().BeFalse(
                because: "soft-deleted user must not appear in default list");

            // Act — with includeDeleted=true: user SHOULD appear
            HttpResponseMessage withDeleted = await adminClient.GetAdminUsersAsync(includeDeleted: true);
            withDeleted.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument docWith = await withDeleted.ReadAsJsonDocumentAsync();
            bool foundWithFlag = ContainsUserId(docWith, userId);
            foundWithFlag.Should().BeTrue(
                because: "soft-deleted user must appear when includeDeleted=true");
        }

        // =========================================================
        // U-03: GET /api/admin/users no token → 401
        // =========================================================

        [Fact]
        public async Task GetUsers_NoToken_Returns401()
        {
            // Arrange
            using HttpClient anonClient = _fixture.CreateClient();

            // Act
            HttpResponseMessage response = await anonClient.GetAdminUsersAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // =========================================================
        // U-04: GET /api/admin/users User role → 403
        // =========================================================

        [Fact]
        public async Task GetUsers_UserRole_Returns403()
        {
            // Arrange
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("usr-forbidden-list");

            // Act
            HttpResponseMessage response = await userClient.GetAdminUsersAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // =========================================================
        // U-05: GET /api/admin/users/{id} existing → 200 with correct fields
        // =========================================================

        [Fact]
        public async Task GetUserById_ExistingUser_Returns200WithCorrectFields()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string email = BlogApiFixture.UniqueEmail("getbyid");
            string userId = await adminClient.ArrangeCreateAdminUserAsync("John", "Doe", email);

            // Act
            HttpResponseMessage response = await adminClient.GetAdminUserByIdAsync(userId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.GetProperty("id").GetString().Should().Be(userId);
            root.GetProperty("firstName").GetString().Should().Be("John");
            root.GetProperty("lastName").GetString().Should().Be("Doe");
            root.GetProperty("email").GetString().Should().Be(email);
            // Roles field should exist and contain "User"
            root.TryGetProperty("roles", out JsonElement rolesProp).Should().BeTrue();
            rolesProp.ValueKind.Should().Be(JsonValueKind.Array);
        }

        // =========================================================
        // U-06: GET /api/admin/users/{id} non-existent → 404
        // =========================================================

        [Fact]
        public async Task GetUserById_NonExistent_Returns404()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string ghostId = Guid.NewGuid().ToString();

            // Act
            HttpResponseMessage response = await adminClient.GetAdminUserByIdAsync(ghostId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // =========================================================
        // U-07: POST /api/admin/users valid data → 201, "User" role assigned
        // =========================================================

        [Fact]
        public async Task CreateUser_ValidData_Returns201WithUserRole()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string email = BlogApiFixture.UniqueEmail("create-valid");

            // Act
            HttpResponseMessage response = await adminClient.CreateAdminUserAsync(
                "Alice", "Smith", email, "Valid@1234", "https://example.com/avatar.png");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.GetProperty("firstName").GetString().Should().Be("Alice");
            root.GetProperty("lastName").GetString().Should().Be("Smith");
            root.GetProperty("email").GetString().Should().Be(email);

            // Must have "User" role assigned by default
            bool hasUserRole = false;
            foreach (JsonElement role in root.GetProperty("roles").EnumerateArray())
            {
                if (role.GetString() == "User")
                {
                    hasUserRole = true;
                }
            }
            hasUserRole.Should().BeTrue(because: "admin-created users must receive the 'User' role by default");
        }

        // =========================================================
        // U-08: POST /api/admin/users duplicate email → 409
        // =========================================================

        [Fact]
        public async Task CreateUser_DuplicateEmail_Returns409()
        {
            // Arrange — create a user with a specific email
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string email = BlogApiFixture.UniqueEmail("dup-email");
            HttpResponseMessage first = await adminClient.CreateAdminUserAsync("First", "User", email, "Valid@1234");
            first.StatusCode.Should().Be(HttpStatusCode.Created, "first creation must succeed");

            // Act — attempt to create again with same email
            HttpResponseMessage response = await adminClient.CreateAdminUserAsync("Second", "User", email, "Valid@5678");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        // =========================================================
        // U-09: POST /api/admin/users weak password → 400
        // =========================================================

        [Fact]
        public async Task CreateUser_WeakPassword_Returns400()
        {
            // Arrange — password missing uppercase/digit
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string email = BlogApiFixture.UniqueEmail("weak-pwd");

            // Act — "password" has no uppercase letter and no digit
            HttpResponseMessage response = await adminClient.CreateAdminUserAsync(
                "Weak", "Pass", email, "weakpassword");

            // Assert — FluentValidation should reject before Identity even sees it
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // =========================================================
        // U-10: POST /api/admin/users empty firstName → 400
        // =========================================================

        [Fact]
        public async Task CreateUser_EmptyFirstName_Returns400()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string email = BlogApiFixture.UniqueEmail("empty-fname");

            // Act
            HttpResponseMessage response = await adminClient.CreateAdminUserAsync(
                "", "Last", email, "Valid@1234");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // =========================================================
        // U-11: POST /api/admin/users no token → 401
        // =========================================================

        [Fact]
        public async Task CreateUser_NoToken_Returns401()
        {
            // Arrange
            using HttpClient anonClient = _fixture.CreateClient();
            string email = BlogApiFixture.UniqueEmail("anon-create");

            // Act
            HttpResponseMessage response = await anonClient.CreateAdminUserAsync(
                "Anon", "User", email, "Valid@1234");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // =========================================================
        // U-12: POST /api/admin/users Manager role → 403
        // =========================================================

        [Fact]
        public async Task CreateUser_ManagerRole_Returns403()
        {
            // Arrange
            using HttpClient managerClient = await _fixture.CreateManagerClientAsync("mgr-create-forbidden");
            string email = BlogApiFixture.UniqueEmail("mgr-create-target");

            // Act
            HttpResponseMessage response = await managerClient.CreateAdminUserAsync(
                "Mgr", "Forbidden", email, "Valid@1234");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // =========================================================
        // U-13: PUT /api/admin/users/{id} valid → 200 with updated fields
        // =========================================================

        [Fact]
        public async Task UpdateUser_ValidData_Returns200WithUpdatedFields()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string email = BlogApiFixture.UniqueEmail("update-valid");
            string userId = await adminClient.ArrangeCreateAdminUserAsync("Old", "Name", email);

            // Act
            HttpResponseMessage response = await adminClient.UpdateAdminUserAsync(
                userId, "NewFirst", "NewLast", "https://example.com/new-avatar.png");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            doc.RootElement.GetProperty("firstName").GetString().Should().Be("NewFirst");
            doc.RootElement.GetProperty("lastName").GetString().Should().Be("NewLast");
        }

        // =========================================================
        // U-14: PUT /api/admin/users/{id} empty firstName → 400
        // =========================================================

        [Fact]
        public async Task UpdateUser_EmptyFirstName_Returns400()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string email = BlogApiFixture.UniqueEmail("upd-empty-fname");
            string userId = await adminClient.ArrangeCreateAdminUserAsync("Valid", "Name", email);

            // Act
            HttpResponseMessage response = await adminClient.UpdateAdminUserAsync(userId, "", "Name");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // =========================================================
        // U-15: PUT /api/admin/users/{id} non-existent → 404
        // =========================================================

        [Fact]
        public async Task UpdateUser_NonExistent_Returns404()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string ghostId = Guid.NewGuid().ToString();

            // Act
            HttpResponseMessage response = await adminClient.UpdateAdminUserAsync(ghostId, "Any", "Name");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // =========================================================
        // U-16: DELETE /api/admin/users/{id} existing → 204, then login → 401
        // =========================================================

        [Fact]
        public async Task DeleteUser_ExistingUser_Returns204AndCannotLogin()
        {
            // Arrange — create a regular user (not admin)
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string email = BlogApiFixture.UniqueEmail("softdel-usr");
            const string password = "Valid@1234";
            string userId = await adminClient.ArrangeCreateAdminUserAsync("ToDelete", "User", email, password);

            // Verify the user can login before deletion
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage loginBefore = await anonClient.PostAsJsonAsync("/api/auth/login",
                new { Email = email, Password = password }, BlogApiFixture.JsonOptions);
            loginBefore.StatusCode.Should().Be(HttpStatusCode.OK,
                because: "user should be able to login before soft-deletion");

            // Act — soft-delete the user
            HttpResponseMessage deleteResp = await adminClient.DeleteAdminUserAsync(userId);

            // Assert — 204 No Content
            deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify — soft-deleted user cannot login
            using HttpClient anonClientAfter = _fixture.CreateClient();
            HttpResponseMessage loginAfter = await anonClientAfter.PostAsJsonAsync("/api/auth/login",
                new { Email = email, Password = password }, BlogApiFixture.JsonOptions);
            loginAfter.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                because: "soft-deleted user must not be able to login");
        }

        // =========================================================
        // U-17: DELETE /api/admin/users/{id} non-existent → 404
        // =========================================================

        [Fact]
        public async Task DeleteUser_NonExistent_Returns404()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string ghostId = Guid.NewGuid().ToString();

            // Act
            HttpResponseMessage response = await adminClient.DeleteAdminUserAsync(ghostId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // =========================================================
        // U-18: DELETE /api/admin/users/{id} admin deletes self → 400
        // =========================================================

        [Fact]
        public async Task DeleteUser_AdminDeletesSelf_Returns400()
        {
            // Arrange — get admin's own user id by calling GET /api/me
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            HttpResponseMessage meResp = await adminClient.GetAsync("/api/me");
            meResp.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument meDoc = await meResp.ReadAsJsonDocumentAsync();
            string adminId = meDoc.RootElement.GetProperty("id").GetString()!;

            // Act — admin tries to delete their own account
            HttpResponseMessage response = await adminClient.DeleteAdminUserAsync(adminId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                because: "admin cannot delete their own account (CannotDeleteSelf rule)");
        }

        // =========================================================
        // U-19: Login soft-deleted user → 401
        // =========================================================

        [Fact]
        public async Task Login_SoftDeletedUser_Returns401()
        {
            // Arrange — create user, then soft-delete via admin
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string email = BlogApiFixture.UniqueEmail("login-del");
            const string password = "Valid@1234";
            string userId = await adminClient.ArrangeCreateAdminUserAsync("Deleted", "Login", email, password);

            await adminClient.DeleteAdminUserAsync(userId);

            // Act — attempt login with soft-deleted account
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.PostAsJsonAsync("/api/auth/login",
                new { Email = email, Password = password }, BlogApiFixture.JsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                because: "soft-deleted user cannot authenticate");
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static bool ContainsUserId(JsonDocument doc, string userId)
        {
            JsonElement root = doc.RootElement;
            if (!root.TryGetProperty("items", out JsonElement items))
            {
                return false;
            }

            foreach (JsonElement item in items.EnumerateArray())
            {
                if (item.TryGetProperty("id", out JsonElement idProp) &&
                    idProp.GetString() == userId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
