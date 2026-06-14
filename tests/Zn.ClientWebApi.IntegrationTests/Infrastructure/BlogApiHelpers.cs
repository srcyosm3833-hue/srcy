using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Zn.ClientWebApi.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Extension methods for Category and Blog HTTP calls.
    /// Mirrors the AuthApiHelpers pattern: thin wrappers that keep test logic readable.
    /// </summary>
    internal static class BlogApiHelpers
    {
        private static readonly JsonSerializerOptions JsonOpts = BlogApiFixture.JsonOptions;

        // ---- Categories (public read) ----

        public static Task<HttpResponseMessage> GetCategoriesAsync(this HttpClient client) =>
            client.GetAsync("/api/categories");

        public static Task<HttpResponseMessage> GetCategoryByIdAsync(this HttpClient client, Guid id) =>
            client.GetAsync($"/api/categories/{id}");

        // ---- Categories (admin write) ----

        public static Task<HttpResponseMessage> CreateCategoryAsync(this HttpClient client, string categoryName) =>
            client.PostAsJsonAsync("/api/admin/categories", new { CategoryName = categoryName }, JsonOpts);

        public static Task<HttpResponseMessage> UpdateCategoryAsync(
            this HttpClient client, Guid id, string categoryName) =>
            client.PutAsJsonAsync($"/api/admin/categories/{id}", new { CategoryName = categoryName }, JsonOpts);

        public static Task<HttpResponseMessage> DeleteCategoryAsync(this HttpClient client, Guid id) =>
            client.DeleteAsync($"/api/admin/categories/{id}");

        // ---- Blogs (public read) ----

        public static Task<HttpResponseMessage> GetBlogsAsync(
            this HttpClient client, int page = 1, int pageSize = 10, Guid? categoryId = null)
        {
            string url = $"/api/blogs?page={page}&pageSize={pageSize}";
            if (categoryId.HasValue)
            {
                url += $"&categoryId={categoryId.Value}";
            }
            return client.GetAsync(url);
        }

        public static Task<HttpResponseMessage> GetBlogByIdAsync(this HttpClient client, Guid id) =>
            client.GetAsync($"/api/blogs/{id}");

        // ---- Blogs (authenticated write) ----

        public static Task<HttpResponseMessage> CreateBlogAsync(
            this HttpClient client,
            string title,
            string description,
            string coverImage,
            string blogImage,
            Guid categoryId) =>
            client.PostAsJsonAsync("/api/blogs", new
            {
                Title = title,
                Description = description,
                CoverImage = coverImage,
                BlogImage = blogImage,
                CategoryId = categoryId
            }, JsonOpts);

        public static Task<HttpResponseMessage> UpdateBlogAsync(
            this HttpClient client,
            Guid id,
            string title,
            string description,
            string coverImage,
            string blogImage,
            Guid categoryId) =>
            client.PutAsJsonAsync($"/api/blogs/{id}", new
            {
                Title = title,
                Description = description,
                CoverImage = coverImage,
                BlogImage = blogImage,
                CategoryId = categoryId
            }, JsonOpts);

        public static Task<HttpResponseMessage> DeleteBlogAsync(this HttpClient client, Guid id) =>
            client.DeleteAsync($"/api/blogs/{id}");

        // ---- Compound arrange helpers ----

        /// <summary>
        /// Creates a category via the admin client and returns its Id.
        /// Throws if the creation fails so arrange failures surface clearly.
        /// </summary>
        public static async Task<Guid> ArrangeCreateCategoryAsync(
            this HttpClient adminClient, string categoryName)
        {
            HttpResponseMessage response = await adminClient.CreateCategoryAsync(categoryName);
            string body = await response.Content.ReadAsStringAsync();
            if ((int)response.StatusCode != 201)
            {
                throw new InvalidOperationException(
                    $"Category creation failed ({response.StatusCode}): {body}");
            }

            JsonDocument doc = JsonDocument.Parse(body);
            string idStr = doc.RootElement.GetProperty("id").GetString()!;
            return Guid.Parse(idStr);
        }

        /// <summary>
        /// Creates a blog via the provided authenticated client and returns its Id.
        /// Throws if the creation fails.
        /// </summary>
        public static async Task<Guid> ArrangeCreateBlogAsync(
            this HttpClient authorClient,
            string title,
            string description,
            Guid categoryId)
        {
            HttpResponseMessage response = await authorClient.CreateBlogAsync(
                title,
                description,
                "https://example.com/cover.png",
                "https://example.com/blog.png",
                categoryId);

            string body = await response.Content.ReadAsStringAsync();
            if ((int)response.StatusCode != 201)
            {
                throw new InvalidOperationException(
                    $"Blog creation failed ({response.StatusCode}): {body}");
            }

            JsonDocument doc = JsonDocument.Parse(body);
            string idStr = doc.RootElement.GetProperty("id").GetString()!;
            return Guid.Parse(idStr);
        }

        // ---- Comments ----

        public static Task<HttpResponseMessage> GetCommentsAsync(
            this HttpClient client, Guid blogId, int page = 1, int pageSize = 10) =>
            client.GetAsync($"/api/blogs/{blogId}/comments?page={page}&pageSize={pageSize}");

        public static Task<HttpResponseMessage> CreateCommentAsync(
            this HttpClient client, Guid blogId, string commentText) =>
            client.PostAsJsonAsync($"/api/blogs/{blogId}/comments",
                new { CommentText = commentText }, JsonOpts);

        public static Task<HttpResponseMessage> UpdateCommentAsync(
            this HttpClient client, Guid blogId, Guid commentId, string commentText) =>
            client.PutAsJsonAsync($"/api/blogs/{blogId}/comments/{commentId}",
                new { CommentText = commentText }, JsonOpts);

        public static Task<HttpResponseMessage> DeleteCommentAsync(
            this HttpClient client, Guid blogId, Guid commentId) =>
            client.DeleteAsync($"/api/blogs/{blogId}/comments/{commentId}");

        /// <summary>
        /// Creates a comment and returns its Id. Throws on failure.
        /// </summary>
        public static async Task<Guid> ArrangeCreateCommentAsync(
            this HttpClient userClient, Guid blogId, string commentText = "A test comment.")
        {
            HttpResponseMessage response = await userClient.CreateCommentAsync(blogId, commentText);
            string body = await response.Content.ReadAsStringAsync();
            if ((int)response.StatusCode != 201)
            {
                throw new InvalidOperationException(
                    $"Comment creation failed ({response.StatusCode}): {body}");
            }

            JsonDocument doc = JsonDocument.Parse(body);
            string idStr = doc.RootElement.GetProperty("id").GetString()!;
            return Guid.Parse(idStr);
        }

        // ---- Replies (SubComments) ----

        public static Task<HttpResponseMessage> CreateReplyAsync(
            this HttpClient client, Guid commentId, string subCommentText) =>
            client.PostAsJsonAsync($"/api/comments/{commentId}/replies",
                new { SubCommentText = subCommentText }, JsonOpts);

        public static Task<HttpResponseMessage> UpdateReplyAsync(
            this HttpClient client, Guid commentId, Guid replyId, string subCommentText) =>
            client.PutAsJsonAsync($"/api/comments/{commentId}/replies/{replyId}",
                new { SubCommentText = subCommentText }, JsonOpts);

        public static Task<HttpResponseMessage> DeleteReplyAsync(
            this HttpClient client, Guid commentId, Guid replyId) =>
            client.DeleteAsync($"/api/comments/{commentId}/replies/{replyId}");

        /// <summary>
        /// Creates a reply and returns its Id. Throws on failure.
        /// </summary>
        public static async Task<Guid> ArrangeCreateReplyAsync(
            this HttpClient userClient, Guid commentId, string subCommentText = "A test reply.")
        {
            HttpResponseMessage response = await userClient.CreateReplyAsync(commentId, subCommentText);
            string body = await response.Content.ReadAsStringAsync();
            if ((int)response.StatusCode != 201)
            {
                throw new InvalidOperationException(
                    $"Reply creation failed ({response.StatusCode}): {body}");
            }

            JsonDocument doc = JsonDocument.Parse(body);
            string idStr = doc.RootElement.GetProperty("id").GetString()!;
            return Guid.Parse(idStr);
        }

        // ---- Messages ----

        public static Task<HttpResponseMessage> SendMessageAsync(
            this HttpClient client,
            string name,
            string email,
            string subject,
            string messageBody) =>
            client.PostAsJsonAsync("/api/messages", new
            {
                Name = name,
                Email = email,
                Subject = subject,
                MessageBody = messageBody
            }, JsonOpts);

        public static Task<HttpResponseMessage> GetAdminMessagesAsync(
            this HttpClient client, int page = 1, int pageSize = 10) =>
            client.GetAsync($"/api/admin/messages?page={page}&pageSize={pageSize}");

        public static Task<HttpResponseMessage> PatchMessageIsReadAsync(
            this HttpClient client, Guid messageId, bool isRead) =>
            client.PatchAsJsonAsync($"/api/admin/messages/{messageId}",
                new { IsRead = isRead }, JsonOpts);

        // ---- Admin Users (Faz 5) ----

        public static Task<HttpResponseMessage> GetAdminUsersAsync(
            this HttpClient client, int page = 1, int pageSize = 20, bool includeDeleted = false)
        {
            string url = $"/api/admin/users?page={page}&pageSize={pageSize}&includeDeleted={includeDeleted}";
            return client.GetAsync(url);
        }

        public static Task<HttpResponseMessage> GetAdminUserByIdAsync(
            this HttpClient client, string userId) =>
            client.GetAsync($"/api/admin/users/{userId}");

        public static Task<HttpResponseMessage> CreateAdminUserAsync(
            this HttpClient client,
            string firstName,
            string lastName,
            string email,
            string password,
            string? imageUrl = null) =>
            client.PostAsJsonAsync("/api/admin/users", new
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = password,
                ImageUrl = imageUrl
            }, JsonOpts);

        public static Task<HttpResponseMessage> UpdateAdminUserAsync(
            this HttpClient client,
            string userId,
            string firstName,
            string lastName,
            string? imageUrl = null) =>
            client.PutAsJsonAsync($"/api/admin/users/{userId}", new
            {
                FirstName = firstName,
                LastName = lastName,
                ImageUrl = imageUrl
            }, JsonOpts);

        public static Task<HttpResponseMessage> DeleteAdminUserAsync(
            this HttpClient client, string userId) =>
            client.DeleteAsync($"/api/admin/users/{userId}");

        // ---- Blog Search (Faz 5) ----

        public static Task<HttpResponseMessage> SearchBlogsAsync(
            this HttpClient client,
            string q,
            int page = 1,
            int pageSize = 10,
            Guid? categoryId = null)
        {
            string url = $"/api/blogs/search?q={Uri.EscapeDataString(q)}&page={page}&pageSize={pageSize}";
            if (categoryId.HasValue)
            {
                url += $"&categoryId={categoryId.Value}";
            }
            return client.GetAsync(url);
        }

        /// <summary>
        /// Creates an admin user via POST /api/admin/users and returns the new user's Id.
        /// Throws if the creation fails.
        /// </summary>
        public static async Task<string> ArrangeCreateAdminUserAsync(
            this HttpClient adminClient,
            string firstName,
            string lastName,
            string email,
            string password = "Valid@1234",
            string? imageUrl = null)
        {
            HttpResponseMessage response = await adminClient.CreateAdminUserAsync(
                firstName, lastName, email, password, imageUrl);
            string body = await response.Content.ReadAsStringAsync();
            if ((int)response.StatusCode != 201)
            {
                throw new InvalidOperationException(
                    $"Admin user creation failed ({response.StatusCode}): {body}");
            }

            JsonDocument doc = JsonDocument.Parse(body);
            return doc.RootElement.GetProperty("id").GetString()!;
        }

        // ---- Blog Like (Faz 5 - A9) ----

        /// <summary>
        /// Toggles a blog like via POST /api/blogs/{id}/like.
        /// Requires an authenticated client (returns 401 for anonymous).
        /// </summary>
        public static Task<HttpResponseMessage> ToggleBlogLikeAsync(this HttpClient client, Guid blogId) =>
            client.PostAsync($"/api/blogs/{blogId}/like", null);

        // ---- Comment Like (Faz 5 - A10) ----

        /// <summary>
        /// Toggles a comment like via POST /api/comments/{id}/like.
        /// Requires an authenticated client (returns 401 for anonymous).
        /// </summary>
        public static Task<HttpResponseMessage> ToggleCommentLikeAsync(this HttpClient client, Guid commentId) =>
            client.PostAsync($"/api/comments/{commentId}/like", null);

        // ---- Admin Comment Moderation (Faz 5) ----

        /// <summary>
        /// Calls GET /api/admin/comments with optional pagination parameters.
        /// Requires an Admin-role Bearer token; returns 401 for anonymous, 403 for non-Admin roles.
        /// </summary>
        public static Task<HttpResponseMessage> GetAdminCommentsAsync(
            this HttpClient client, int page = 1, int pageSize = 20) =>
            client.GetAsync($"/api/admin/comments?page={page}&pageSize={pageSize}");

        // ---- Admin Blog Audit (Audit / IP hash) ----

        /// <summary>
        /// Calls GET /api/admin/blogs/{id} to retrieve a blog's audit detail (includes creatorIpHash).
        /// Requires Admin or Manager token.
        /// </summary>
        public static Task<HttpResponseMessage> GetAdminBlogAuditAsync(this HttpClient client, Guid blogId) =>
            client.GetAsync($"/api/admin/blogs/{blogId}");

        // ---- Blog creation with X-Forwarded-For header ----

        /// <summary>
        /// Creates a blog via POST /api/blogs with an explicit X-Forwarded-For header so the
        /// IP resolver picks up the provided IP and hashes it into creatorIpHash.
        /// </summary>
        public static Task<HttpResponseMessage> CreateBlogWithIpAsync(
            this HttpClient client,
            string title,
            string description,
            Guid categoryId,
            string forwardedForIp)
        {
            var request = new System.Net.Http.HttpRequestMessage(
                System.Net.Http.HttpMethod.Post, "/api/blogs");
            request.Headers.Add("X-Forwarded-For", forwardedForIp);
            request.Content = System.Net.Http.Json.JsonContent.Create(new
            {
                Title = title,
                Description = description,
                CoverImage = "https://example.com/cover.png",
                BlogImage = "https://example.com/blog.png",
                CategoryId = categoryId
            }, options: JsonOpts);
            return client.SendAsync(request);
        }

        // ---- Message with X-Forwarded-For header ----

        /// <summary>
        /// Sends a message via POST /api/messages with an explicit X-Forwarded-For header so the
        /// IP resolver picks up the provided IP and hashes it into senderIpHash.
        /// </summary>
        public static Task<HttpResponseMessage> SendMessageWithIpAsync(
            this HttpClient client,
            string name,
            string email,
            string subject,
            string body,
            string forwardedForIp)
        {
            var request = new System.Net.Http.HttpRequestMessage(
                System.Net.Http.HttpMethod.Post, "/api/messages");
            request.Headers.Add("X-Forwarded-For", forwardedForIp);
            request.Content = System.Net.Http.Json.JsonContent.Create(new
            {
                Name = name,
                Email = email,
                Subject = subject,
                MessageBody = body
            }, options: JsonOpts);
            return client.SendAsync(request);
        }

        // ---- Search logs (Admin audit) ----

        /// <summary>
        /// Calls GET /api/admin/search-logs with optional pagination and term filter parameters.
        /// Requires Admin-role Bearer token; returns 401 for anonymous, 403 for Manager/User.
        /// </summary>
        public static Task<HttpResponseMessage> GetAdminSearchLogsAsync(
            this HttpClient client,
            int page = 1,
            int pageSize = 20,
            string? term = null)
        {
            string url = $"/api/admin/search-logs?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(term))
            {
                url += $"&term={Uri.EscapeDataString(term)}";
            }
            return client.GetAsync(url);
        }

        // ---- Blog search with X-Forwarded-For header ----

        /// <summary>
        /// Searches blogs with an explicit X-Forwarded-For header to force IP resolution for audit logging.
        /// </summary>
        public static Task<HttpResponseMessage> SearchBlogsWithIpAsync(
            this HttpClient client,
            string q,
            string forwardedForIp,
            int page = 1,
            int pageSize = 10)
        {
            var request = new System.Net.Http.HttpRequestMessage(
                System.Net.Http.HttpMethod.Get,
                $"/api/blogs/search?q={Uri.EscapeDataString(q)}&page={page}&pageSize={pageSize}");
            request.Headers.Add("X-Forwarded-For", forwardedForIp);
            return client.SendAsync(request);
        }

        // ---- Role management (Audit Faz) ----

        /// <summary>
        /// Calls GET /api/admin/roles to list all roles with userCount and isProtected.
        /// Requires Admin-role Bearer token.
        /// </summary>
        public static Task<HttpResponseMessage> GetAdminRolesAsync(this HttpClient client) =>
            client.GetAsync("/api/admin/roles");

        /// <summary>
        /// Calls POST /api/admin/roles to create a new custom role.
        /// Requires Admin-role Bearer token.
        /// </summary>
        public static Task<HttpResponseMessage> CreateAdminRoleAsync(this HttpClient client, string roleName) =>
            client.PostAsJsonAsync("/api/admin/roles", new { Name = roleName }, JsonOpts);

        /// <summary>
        /// Calls PUT /api/admin/roles/{id} to rename an existing role.
        /// Requires Admin-role Bearer token.
        /// </summary>
        public static Task<HttpResponseMessage> UpdateAdminRoleAsync(
            this HttpClient client, string roleId, string newName) =>
            client.PutAsJsonAsync($"/api/admin/roles/{roleId}", new { Name = newName }, JsonOpts);

        /// <summary>
        /// Calls DELETE /api/admin/roles/{id} to delete a custom role.
        /// Requires Admin-role Bearer token.
        /// </summary>
        public static Task<HttpResponseMessage> DeleteAdminRoleAsync(
            this HttpClient client, string roleId) =>
            client.DeleteAsync($"/api/admin/roles/{roleId}");

        /// <summary>
        /// Creates a custom role via POST /api/admin/roles and returns the role's Id.
        /// Throws if creation fails.
        /// </summary>
        public static async Task<string> ArrangeCreateAdminRoleAsync(
            this HttpClient adminClient, string roleName)
        {
            HttpResponseMessage response = await adminClient.CreateAdminRoleAsync(roleName);
            string body = await response.Content.ReadAsStringAsync();
            if ((int)response.StatusCode != 201)
            {
                throw new InvalidOperationException(
                    $"Role creation failed ({response.StatusCode}): {body}");
            }

            JsonDocument doc = JsonDocument.Parse(body);
            return doc.RootElement.GetProperty("id").GetString()!;
        }

        /// <summary>
        /// Calls POST /api/admin/users/{id}/roles to assign a role to a user.
        /// Requires Admin-role Bearer token.
        /// </summary>
        public static Task<HttpResponseMessage> AssignUserRoleAsync(
            this HttpClient client, string userId, string roleName) =>
            client.PostAsJsonAsync($"/api/admin/users/{userId}/roles",
                new { RoleName = roleName }, JsonOpts);

        /// <summary>
        /// Calls DELETE /api/admin/users/{id}/roles/{roleName} to remove a role from a user.
        /// Requires Admin-role Bearer token.
        /// </summary>
        public static Task<HttpResponseMessage> RemoveUserRoleAsync(
            this HttpClient client, string userId, string roleName) =>
            client.DeleteAsync($"/api/admin/users/{userId}/roles/{roleName}");

    }
}
