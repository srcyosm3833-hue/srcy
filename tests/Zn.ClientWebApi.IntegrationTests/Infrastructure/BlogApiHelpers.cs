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

    }
}
