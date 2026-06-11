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

    }
}
