using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.Audit
{
    /// <summary>
    /// Integration tests for Role CRUD and User Role Assignment endpoints (Faz 5 — A6).
    ///
    /// Covered endpoints:
    ///   GET    /api/admin/roles              → list all roles with userCount + isProtected
    ///   POST   /api/admin/roles              → create custom role (Admin only)
    ///   PUT    /api/admin/roles/{id}         → rename role (Admin only)
    ///   DELETE /api/admin/roles/{id}         → delete role (Admin only)
    ///   POST   /api/admin/users/{id}/roles   → assign role to user (Admin only)
    ///   DELETE /api/admin/users/{id}/roles/{roleName} → remove role from user (Admin only)
    ///
    /// Covered scenarios:
    ///   RM-01. GET /api/admin/roles Admin → 200, contains Admin+Manager+User with isProtected=true
    ///   RM-02. GET /api/admin/roles → userCount for a role with a known user is correct
    ///   RM-03. GET /api/admin/roles Manager token → 403 (Admin-only)
    ///   RM-04. GET /api/admin/roles anonymous → 401
    ///   RM-05. POST /api/admin/roles unique name → 201 + RoleResponse (isProtected=false, userCount=0)
    ///   RM-06. POST /api/admin/roles duplicate name → 409
    ///   RM-07. POST /api/admin/roles Manager token → 403
    ///   RM-08. POST /api/admin/roles empty name → 400
    ///   RM-09. PUT /api/admin/roles/{id} valid rename → 200 + updated name
    ///   RM-10. PUT /api/admin/roles/{id} protected role (Admin) → 400
    ///   RM-11. PUT /api/admin/roles/{id} non-existent → 404
    ///   RM-12. PUT /api/admin/roles/{id} duplicate name → 409
    ///   RM-13. DELETE /api/admin/roles/{id} empty custom role → 204
    ///   RM-14. DELETE /api/admin/roles/{id} protected role → 400
    ///   RM-15. DELETE /api/admin/roles/{id} role with users → 409
    ///   RM-16. DELETE /api/admin/roles/{id} non-existent → 404
    ///   RM-17. AssignRole happy path: user gets new role → 200, roles list updated
    ///   RM-18. AssignRole idempotent: assign same role twice → 200 (no error)
    ///   RM-19. AssignRole Manager token → 403 (Admin-only per A6)
    ///   RM-20. AssignRole anonymous → 401
    ///   RM-21. AssignRole non-existent user → 404
    ///   RM-22. AssignRole non-existent role → 404
    ///   RM-23. RemoveRole happy path: role removed from user → 204
    ///   RM-24. RemoveRole last Admin protection: removing Admin from only admin → 400
    ///   RM-25. RemoveRole non-existent user → 404
    ///   RM-26. RemoveRole non-existent role → 404
    /// </summary>
    [Collection(BlogApiCollection.CollectionName)]
    public sealed class RoleManagementTests
    {
        private readonly BlogApiFixture _fixture;

        public RoleManagementTests(BlogApiFixture fixture)
        {
            _fixture = fixture;
        }

        // =========================================================
        // RM-01: GET /api/admin/roles → 200 + protected system roles visible
        // =========================================================

        [Fact]
        public async Task GetRoles_AdminToken_Returns200WithProtectedSystemRoles()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();

            // Act
            HttpResponseMessage response = await adminClient.GetAdminRolesAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            JsonElement root = doc.RootElement;

            // Response is an array (IReadOnlyList<RoleResponse>)
            root.ValueKind.Should().Be(JsonValueKind.Array,
                because: "GET /api/admin/roles returns an array of role objects");

            bool foundAdmin = false;
            bool foundManager = false;
            bool foundUser = false;

            foreach (JsonElement role in root.EnumerateArray())
            {
                string? name = role.GetProperty("name").GetString();
                bool isProtected = role.GetProperty("isProtected").GetBoolean();

                if (name == "Admin") { foundAdmin = true; isProtected.Should().BeTrue("Admin is a protected role"); }
                if (name == "Manager") { foundManager = true; isProtected.Should().BeTrue("Manager is a protected role"); }
                if (name == "User") { foundUser = true; isProtected.Should().BeTrue("User is a protected role"); }

                // Every role must have the required fields
                role.TryGetProperty("id", out _).Should().BeTrue("id field must be present");
                role.TryGetProperty("userCount", out _).Should().BeTrue("userCount field must be present");
            }

            foundAdmin.Should().BeTrue("Admin role must appear in the list");
            foundManager.Should().BeTrue("Manager role must appear in the list");
            foundUser.Should().BeTrue("User role must appear in the list");
        }

        // =========================================================
        // RM-02: userCount for a role reflects actual members
        // =========================================================

        [Fact]
        public async Task GetRoles_UserCountReflectsActualMembers()
        {
            // Arrange — create a custom role and assign a user to it
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string roleName = $"CountCheckRole-{Guid.NewGuid():N}";
            string roleId = await adminClient.ArrangeCreateAdminRoleAsync(roleName);

            // Create a user to assign to this role
            string email = BlogApiFixture.UniqueEmail("rc02-user");
            string userId = await adminClient.ArrangeCreateAdminUserAsync(
                "RoleCount", "User", email);

            // Assign the role
            HttpResponseMessage assignResp = await adminClient.AssignUserRoleAsync(userId, roleName);
            assignResp.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act
            HttpResponseMessage listResp = await adminClient.GetAdminRolesAsync();
            listResp.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument doc = JsonDocument.Parse(await listResp.Content.ReadAsStringAsync());

            // Assert — find our custom role and verify userCount >= 1
            JsonElement? customRole = FindRoleByName(doc.RootElement, roleName);
            customRole.Should().NotBeNull(
                because: "the custom role we created must appear in the list");

            customRole!.Value.GetProperty("userCount").GetInt32().Should().BeGreaterThanOrEqualTo(1,
                because: "the custom role has at least one user assigned to it");

            // Clean up — delete role after removing user from it
            await adminClient.RemoveUserRoleAsync(userId, roleName);
            await adminClient.DeleteAdminRoleAsync(roleId);
        }

        // =========================================================
        // RM-03: GET /api/admin/roles Manager token → 403
        // =========================================================

        [Fact]
        public async Task GetRoles_ManagerToken_Returns403()
        {
            // Arrange
            using HttpClient managerClient = await _fixture.CreateManagerClientAsync("rm03-mgr");

            // Act
            HttpResponseMessage response = await managerClient.GetAdminRolesAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
                because: "GET /api/admin/roles is Admin-only per A6 permission matrix");
        }

        // =========================================================
        // RM-04: GET /api/admin/roles anonymous → 401
        // =========================================================

        [Fact]
        public async Task GetRoles_Anonymous_Returns401()
        {
            // Arrange
            using HttpClient anonClient = _fixture.CreateClient();

            // Act
            HttpResponseMessage response = await anonClient.GetAdminRolesAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // =========================================================
        // RM-05: POST /api/admin/roles unique name → 201 + RoleResponse
        // =========================================================

        [Fact]
        public async Task CreateRole_UniqueName_Returns201WithRoleResponse()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string roleName = $"CustomRole-{Guid.NewGuid():N}";

            // Act
            HttpResponseMessage response = await adminClient.CreateAdminRoleAsync(roleName);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created,
                because: "creating a role with a unique name must succeed");

            JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            JsonElement root = doc.RootElement;

            root.GetProperty("name").GetString().Should().Be(roleName);
            root.GetProperty("userCount").GetInt32().Should().Be(0,
                because: "a newly created role has no users assigned");
            root.GetProperty("isProtected").GetBoolean().Should().BeFalse(
                because: "a custom role created via the API is not protected");
            root.TryGetProperty("id", out JsonElement idProp).Should().BeTrue();
            idProp.GetString().Should().NotBeNullOrWhiteSpace();

            // Cleanup
            string roleId = idProp.GetString()!;
            await adminClient.DeleteAdminRoleAsync(roleId);
        }

        // =========================================================
        // RM-06: POST /api/admin/roles duplicate name → 409
        // =========================================================

        [Fact]
        public async Task CreateRole_DuplicateName_Returns409()
        {
            // Arrange — create a role first
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string roleName = $"DupRole-{Guid.NewGuid():N}";
            string roleId = await adminClient.ArrangeCreateAdminRoleAsync(roleName);

            // Act — try to create again with the same name
            HttpResponseMessage response = await adminClient.CreateAdminRoleAsync(roleName);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict,
                because: "creating a role with a duplicate name must return 409");

            // Cleanup
            await adminClient.DeleteAdminRoleAsync(roleId);
        }

        // =========================================================
        // RM-07: POST /api/admin/roles Manager token → 403
        // =========================================================

        [Fact]
        public async Task CreateRole_ManagerToken_Returns403()
        {
            // Arrange
            using HttpClient managerClient = await _fixture.CreateManagerClientAsync("rm07-mgr");

            // Act
            HttpResponseMessage response = await managerClient.CreateAdminRoleAsync("AnyRoleName");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
                because: "POST /api/admin/roles is Admin-only per A6 permission matrix");
        }

        // =========================================================
        // RM-08: POST /api/admin/roles empty name → 400
        // =========================================================

        [Fact]
        public async Task CreateRole_EmptyName_Returns400()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();

            // Act
            HttpResponseMessage response = await adminClient.CreateAdminRoleAsync("");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                because: "empty role name must be rejected by FluentValidation");
        }

        // =========================================================
        // RM-09: PUT /api/admin/roles/{id} valid rename → 200 + updated name
        // =========================================================

        [Fact]
        public async Task UpdateRole_ValidRename_Returns200WithUpdatedName()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string originalName = $"OriginalRole-{Guid.NewGuid():N}";
            string newName = $"RenamedRole-{Guid.NewGuid():N}";
            string roleId = await adminClient.ArrangeCreateAdminRoleAsync(originalName);

            // Act
            HttpResponseMessage response = await adminClient.UpdateAdminRoleAsync(roleId, newName);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                because: "renaming a custom role must succeed");

            JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            doc.RootElement.GetProperty("name").GetString().Should().Be(newName,
                because: "the updated role name must be reflected in the response");

            // Cleanup
            await adminClient.DeleteAdminRoleAsync(roleId);
        }

        // =========================================================
        // RM-10: PUT /api/admin/roles/{id} protected role → 400
        // =========================================================

        [Fact]
        public async Task UpdateRole_ProtectedRole_Returns400()
        {
            // Arrange — we need the ID of the "Admin" role
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            HttpResponseMessage listResp = await adminClient.GetAdminRolesAsync();
            listResp.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument listDoc = JsonDocument.Parse(await listResp.Content.ReadAsStringAsync());
            JsonElement? adminRole = FindRoleByName(listDoc.RootElement, "Admin");
            adminRole.Should().NotBeNull("Admin role must exist in the list");
            string adminRoleId = adminRole!.Value.GetProperty("id").GetString()!;

            // Act
            HttpResponseMessage response = await adminClient.UpdateAdminRoleAsync(adminRoleId, "NotAdmin");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                because: "a protected system role (Admin) cannot be renamed");
        }

        // =========================================================
        // RM-11: PUT /api/admin/roles/{id} non-existent → 404
        // =========================================================

        [Fact]
        public async Task UpdateRole_NonExistent_Returns404()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string ghostId = Guid.NewGuid().ToString();

            // Act
            HttpResponseMessage response = await adminClient.UpdateAdminRoleAsync(ghostId, "AnyName");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // =========================================================
        // RM-12: PUT /api/admin/roles/{id} duplicate name → 409
        // =========================================================

        [Fact]
        public async Task UpdateRole_DuplicateName_Returns409()
        {
            // Arrange — two custom roles
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string nameA = $"RoleA-{Guid.NewGuid():N}";
            string nameB = $"RoleB-{Guid.NewGuid():N}";
            string idA = await adminClient.ArrangeCreateAdminRoleAsync(nameA);
            string idB = await adminClient.ArrangeCreateAdminRoleAsync(nameB);

            // Act — try to rename B to A's name (conflict)
            HttpResponseMessage response = await adminClient.UpdateAdminRoleAsync(idB, nameA);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict,
                because: "renaming to a name that already exists must return 409");

            // Cleanup
            await adminClient.DeleteAdminRoleAsync(idA);
            await adminClient.DeleteAdminRoleAsync(idB);
        }

        // =========================================================
        // RM-13: DELETE /api/admin/roles/{id} empty custom role → 204
        // =========================================================

        [Fact]
        public async Task DeleteRole_EmptyCustomRole_Returns204()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string roleName = $"ToDeleteRole-{Guid.NewGuid():N}";
            string roleId = await adminClient.ArrangeCreateAdminRoleAsync(roleName);

            // Act
            HttpResponseMessage response = await adminClient.DeleteAdminRoleAsync(roleId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent,
                because: "deleting an empty custom role must succeed with 204");

            // Verify the role is gone
            HttpResponseMessage listResp = await adminClient.GetAdminRolesAsync();
            JsonDocument listDoc = JsonDocument.Parse(await listResp.Content.ReadAsStringAsync());
            JsonElement? deletedRole = FindRoleByName(listDoc.RootElement, roleName);
            deletedRole.Should().BeNull(
                because: "deleted role must no longer appear in the list");
        }

        // =========================================================
        // RM-14: DELETE /api/admin/roles/{id} protected role → 400
        // =========================================================

        [Fact]
        public async Task DeleteRole_ProtectedRole_Returns400()
        {
            // Arrange — get the ID of the "Manager" protected role
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            HttpResponseMessage listResp = await adminClient.GetAdminRolesAsync();
            JsonDocument listDoc = JsonDocument.Parse(await listResp.Content.ReadAsStringAsync());
            JsonElement? managerRole = FindRoleByName(listDoc.RootElement, "Manager");
            managerRole.Should().NotBeNull("Manager role must exist");
            string managerRoleId = managerRole!.Value.GetProperty("id").GetString()!;

            // Act
            HttpResponseMessage response = await adminClient.DeleteAdminRoleAsync(managerRoleId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                because: "a protected system role (Manager) cannot be deleted");
        }

        // =========================================================
        // RM-15: DELETE /api/admin/roles/{id} role with users → 409
        // =========================================================

        [Fact]
        public async Task DeleteRole_RoleWithUsers_Returns409()
        {
            // Arrange — create a custom role and assign a user to it
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string roleName = $"OccupiedRole-{Guid.NewGuid():N}";
            string roleId = await adminClient.ArrangeCreateAdminRoleAsync(roleName);

            string email = BlogApiFixture.UniqueEmail("rm15-user");
            string userId = await adminClient.ArrangeCreateAdminUserAsync("Role", "User", email);

            HttpResponseMessage assignResp = await adminClient.AssignUserRoleAsync(userId, roleName);
            assignResp.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act — try to delete the role that has a user
            HttpResponseMessage response = await adminClient.DeleteAdminRoleAsync(roleId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict,
                because: "a role with active users cannot be deleted; admin must first remove users");

            // Cleanup — remove user from role, then delete role
            await adminClient.RemoveUserRoleAsync(userId, roleName);
            await adminClient.DeleteAdminRoleAsync(roleId);
        }

        // =========================================================
        // RM-16: DELETE /api/admin/roles/{id} non-existent → 404
        // =========================================================

        [Fact]
        public async Task DeleteRole_NonExistent_Returns404()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string ghostId = Guid.NewGuid().ToString();

            // Act
            HttpResponseMessage response = await adminClient.DeleteAdminRoleAsync(ghostId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // =========================================================
        // RM-17: AssignRole happy path → 200, user now has the role
        // =========================================================

        [Fact]
        public async Task AssignRole_HappyPath_Returns200AndUserHasRole()
        {
            // Arrange — create a user and a custom role
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string email = BlogApiFixture.UniqueEmail("rm17-user");
            string userId = await adminClient.ArrangeCreateAdminUserAsync("Assign", "Role", email);
            string roleName = $"AssignTarget-{Guid.NewGuid():N}";
            string roleId = await adminClient.ArrangeCreateAdminRoleAsync(roleName);

            // Act
            HttpResponseMessage response = await adminClient.AssignUserRoleAsync(userId, roleName);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                because: "assigning a valid role to an existing user must succeed");

            JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            JsonElement roles = doc.RootElement.GetProperty("roles");

            bool hasRole = false;
            foreach (JsonElement role in roles.EnumerateArray())
            {
                if (role.GetString() == roleName) hasRole = true;
            }

            hasRole.Should().BeTrue(
                because: "the response must list the newly assigned role in the user's roles");

            // Cleanup
            await adminClient.RemoveUserRoleAsync(userId, roleName);
            await adminClient.DeleteAdminRoleAsync(roleId);
        }

        // =========================================================
        // RM-18: AssignRole idempotent — assigning same role twice returns 200 (no error)
        // =========================================================

        [Fact]
        public async Task AssignRole_Idempotent_SecondAssignReturns200()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string email = BlogApiFixture.UniqueEmail("rm18-user");
            string userId = await adminClient.ArrangeCreateAdminUserAsync("Idem", "Potent", email);
            string roleName = $"IdempotentRole-{Guid.NewGuid():N}";
            string roleId = await adminClient.ArrangeCreateAdminRoleAsync(roleName);

            // Assign once
            HttpResponseMessage first = await adminClient.AssignUserRoleAsync(userId, roleName);
            first.StatusCode.Should().Be(HttpStatusCode.OK, "first assignment must succeed");

            // Act — assign again (idempotent)
            HttpResponseMessage second = await adminClient.AssignUserRoleAsync(userId, roleName);

            // Assert
            second.StatusCode.Should().Be(HttpStatusCode.OK,
                because: "assigning a role the user already has must be idempotent (200, not an error)");

            // Cleanup
            await adminClient.RemoveUserRoleAsync(userId, roleName);
            await adminClient.DeleteAdminRoleAsync(roleId);
        }

        // =========================================================
        // RM-19: AssignRole Manager token → 403 (Admin-only per A6)
        // =========================================================

        [Fact]
        public async Task AssignRole_ManagerToken_Returns403()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string email = BlogApiFixture.UniqueEmail("rm19-target");
            string userId = await adminClient.ArrangeCreateAdminUserAsync("Target", "User", email);

            using HttpClient managerClient = await _fixture.CreateManagerClientAsync("rm19-mgr");

            // Act
            HttpResponseMessage response = await managerClient.AssignUserRoleAsync(userId, "User");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
                because: "POST /api/admin/users/{id}/roles is Admin-only per A6 permission matrix");
        }

        // =========================================================
        // RM-20: AssignRole anonymous → 401
        // =========================================================

        [Fact]
        public async Task AssignRole_Anonymous_Returns401()
        {
            // Arrange
            using HttpClient anonClient = _fixture.CreateClient();
            string ghostUserId = Guid.NewGuid().ToString();

            // Act
            HttpResponseMessage response = await anonClient.AssignUserRoleAsync(ghostUserId, "User");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // =========================================================
        // RM-21: AssignRole non-existent user → 404
        // =========================================================

        [Fact]
        public async Task AssignRole_NonExistentUser_Returns404()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string ghostId = Guid.NewGuid().ToString();

            // Act
            HttpResponseMessage response = await adminClient.AssignUserRoleAsync(ghostId, "User");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound,
                because: "assigning a role to a non-existent user must return 404");
        }

        // =========================================================
        // RM-22: AssignRole non-existent role → 404
        // =========================================================

        [Fact]
        public async Task AssignRole_NonExistentRole_Returns404()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string email = BlogApiFixture.UniqueEmail("rm22-user");
            string userId = await adminClient.ArrangeCreateAdminUserAsync("Ghost", "Role", email);

            // Act
            HttpResponseMessage response = await adminClient.AssignUserRoleAsync(userId, "NonExistentRole");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound,
                because: "assigning a role that does not exist must return 404");
        }

        // =========================================================
        // RM-23: RemoveRole happy path → 204
        // =========================================================

        [Fact]
        public async Task RemoveRole_HappyPath_Returns204()
        {
            // Arrange — create a user and assign a custom role
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string email = BlogApiFixture.UniqueEmail("rm23-user");
            string userId = await adminClient.ArrangeCreateAdminUserAsync("Remove", "Role", email);
            string roleName = $"RemoveTarget-{Guid.NewGuid():N}";
            string roleId = await adminClient.ArrangeCreateAdminRoleAsync(roleName);

            HttpResponseMessage assignResp = await adminClient.AssignUserRoleAsync(userId, roleName);
            assignResp.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act
            HttpResponseMessage response = await adminClient.RemoveUserRoleAsync(userId, roleName);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent,
                because: "removing a role from a user must succeed with 204");

            // Verify the role is gone from user's roles
            HttpResponseMessage userResp = await adminClient.GetAdminUserByIdAsync(userId);
            userResp.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument userDoc = JsonDocument.Parse(await userResp.Content.ReadAsStringAsync());
            JsonElement roles = userDoc.RootElement.GetProperty("roles");

            bool stillHasRole = false;
            foreach (JsonElement role in roles.EnumerateArray())
            {
                if (role.GetString() == roleName) stillHasRole = true;
            }

            stillHasRole.Should().BeFalse(
                because: "the removed role must no longer appear in the user's roles");

            // Cleanup
            await adminClient.DeleteAdminRoleAsync(roleId);
        }

        // =========================================================
        // RM-24: RemoveRole — last Admin protection prevents removing Admin from the only admin
        // =========================================================

        [Fact]
        public async Task RemoveRole_LastAdminRemoveAdminRole_Returns400()
        {
            // Arrange — get the admin user's id (there is only one admin in the isolated test DB)
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            HttpResponseMessage meResp = await adminClient.GetAsync("/api/me");
            meResp.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument meDoc = JsonDocument.Parse(await meResp.Content.ReadAsStringAsync());
            string adminUserId = meDoc.RootElement.GetProperty("id").GetString()!;

            // Act — try to remove Admin role from the only admin
            HttpResponseMessage response = await adminClient.RemoveUserRoleAsync(adminUserId, "Admin");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                because: "the system must prevent removing the Admin role from the last remaining administrator");
        }

        // =========================================================
        // RM-25: RemoveRole non-existent user → 404
        // =========================================================

        [Fact]
        public async Task RemoveRole_NonExistentUser_Returns404()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string ghostId = Guid.NewGuid().ToString();

            // Act
            HttpResponseMessage response = await adminClient.RemoveUserRoleAsync(ghostId, "User");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound,
                because: "removing a role from a non-existent user must return 404");
        }

        // =========================================================
        // RM-26: RemoveRole non-existent role → 404
        // =========================================================

        [Fact]
        public async Task RemoveRole_NonExistentRole_Returns404()
        {
            // Arrange
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            string email = BlogApiFixture.UniqueEmail("rm26-user");
            string userId = await adminClient.ArrangeCreateAdminUserAsync("Ghost", "Role2", email);

            // Act
            HttpResponseMessage response = await adminClient.RemoveUserRoleAsync(userId, "GhostRole");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound,
                because: "removing a role that does not exist from any user must return 404");
        }

        // ------------------------------------------------------------------
        // Private helpers
        // ------------------------------------------------------------------

        private static JsonElement? FindRoleByName(JsonElement rolesArray, string name)
        {
            foreach (JsonElement role in rolesArray.EnumerateArray())
            {
                if (role.TryGetProperty("name", out JsonElement nameProp) &&
                    string.Equals(nameProp.GetString(), name, StringComparison.OrdinalIgnoreCase))
                {
                    return role;
                }
            }

            return null;
        }
    }
}
