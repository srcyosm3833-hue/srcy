using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Zn.ClientWebApi.IntegrationTests.Infrastructure;

namespace Zn.ClientWebApi.IntegrationTests.Messages
{
    /// <summary>
    /// Integration tests for Message endpoints (Faz 3 – Dilim B).
    ///
    /// Covered scenarios:
    ///   M-1.  POST /api/messages valid anonymous → 201, no body (minimal response)
    ///   M-2.  POST invalid email format → 400
    ///   M-3.  POST MessageBody > 2000 chars → 400
    ///   M-4.  POST empty required fields (Name, Subject, MessageBody, Email) → 400 each
    ///   M-5.  GET /api/admin/messages without token → 401
    ///   M-6.  GET /api/admin/messages with normal user token → 403
    ///   M-7.  GET /api/admin/messages with admin token → 200 + paged result
    ///   M-8.  Admin message sort order: unread messages appear before read messages
    ///   M-9.  PATCH /api/admin/messages/{id} set isRead:true → 200, isRead=true
    ///   M-10. PATCH /api/admin/messages/{id} set isRead:false (revert) → 200, isRead=false
    ///   M-11. PATCH non-existent messageId → 404
    /// </summary>
    [Collection(BlogApiCollection.CollectionName)]
    public sealed class MessageTests
    {
        private readonly BlogApiFixture _fixture;

        public MessageTests(BlogApiFixture fixture)
        {
            _fixture = fixture;
        }

        // =========================================================
        // M-1: POST valid anonymous message → 201 (minimal/no body)
        // =========================================================

        [Fact]
        public async Task SendMessage_ValidAnonymous_Returns201WithMinimalBody()
        {
            // Act
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.SendMessageAsync(
                name: "John Doe",
                email: "john.doe@example.com",
                subject: "Hello from integration test",
                messageBody: "This is a test message body.");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created,
                because: "any anonymous visitor can send a message");

            // Response should be minimal — no body / empty body is acceptable
            string body = await response.Content.ReadAsStringAsync();
            // Body should either be empty or not contain any sensitive information like message id
            if (!string.IsNullOrWhiteSpace(body) && body != "null")
            {
                // If there is a body, it must NOT expose the message Id (by design)
                body.Should().NotContainEquivalentOf("\"id\"",
                    because: "message Id must not be leaked to anonymous senders");
            }
        }

        // =========================================================
        // M-2: POST invalid email format → 400
        // =========================================================

        [Fact]
        public async Task SendMessage_InvalidEmailFormat_Returns400()
        {
            // Act
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.SendMessageAsync(
                name: "Jane Doe",
                email: "not-a-valid-email",
                subject: "Subject",
                messageBody: "Message body.");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // =========================================================
        // M-3: POST MessageBody > 2000 chars → 400
        // =========================================================

        [Fact]
        public async Task SendMessage_MessageBodyExceeds2000Chars_Returns400()
        {
            string tooLongBody = new string('z', 2001); // 1 over the 2000-char limit

            // Act
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.SendMessageAsync(
                name: "Spammer",
                email: "spam@example.com",
                subject: "Spam subject",
                messageBody: tooLongBody);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                because: "MessageBody exceeding 2000 chars violates the anti-spam length limit");
        }

        // =========================================================
        // M-4a: POST with empty Name → 400
        // =========================================================

        [Fact]
        public async Task SendMessage_EmptyName_Returns400()
        {
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.SendMessageAsync(
                name: string.Empty,
                email: "valid@example.com",
                subject: "Subject",
                messageBody: "Body.");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // =========================================================
        // M-4b: POST with empty Email → 400
        // =========================================================

        [Fact]
        public async Task SendMessage_EmptyEmail_Returns400()
        {
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.SendMessageAsync(
                name: "Jane",
                email: string.Empty,
                subject: "Subject",
                messageBody: "Body.");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // =========================================================
        // M-4c: POST with empty Subject → 400
        // =========================================================

        [Fact]
        public async Task SendMessage_EmptySubject_Returns400()
        {
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.SendMessageAsync(
                name: "Jane",
                email: "jane@example.com",
                subject: string.Empty,
                messageBody: "Body.");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // =========================================================
        // M-4d: POST with empty MessageBody → 400
        // =========================================================

        [Fact]
        public async Task SendMessage_EmptyMessageBody_Returns400()
        {
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.SendMessageAsync(
                name: "Jane",
                email: "jane@example.com",
                subject: "Subject",
                messageBody: string.Empty);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // =========================================================
        // M-5: GET /api/admin/messages without token → 401
        // =========================================================

        [Fact]
        public async Task GetAdminMessages_NoToken_Returns401()
        {
            using HttpClient anonClient = _fixture.CreateClient();
            HttpResponseMessage response = await anonClient.GetAdminMessagesAsync();

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // =========================================================
        // M-6: GET /api/admin/messages with normal user token → 403
        // =========================================================

        [Fact]
        public async Task GetAdminMessages_NormalUser_Returns403()
        {
            (HttpClient userClient, _, _) = await _fixture.CreateUserClientAsync("msg-m6-user");
            HttpResponseMessage response = await userClient.GetAdminMessagesAsync();

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // =========================================================
        // M-7: GET /api/admin/messages with admin token → 200 + paged result
        // =========================================================

        [Fact]
        public async Task GetAdminMessages_Admin_Returns200WithPagedResult()
        {
            // Arrange — send a message so the list is non-empty
            using HttpClient anonClient = _fixture.CreateClient();
            await anonClient.SendMessageAsync(
                "Tester M7", "m7@example.com", "Test M7", "Body M7.");

            // Act
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            HttpResponseMessage response = await adminClient.GetAdminMessagesAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            JsonElement root = doc.RootElement;

            root.TryGetProperty("items", out _).Should().BeTrue();
            root.TryGetProperty("totalCount", out _).Should().BeTrue();
            root.TryGetProperty("page", out _).Should().BeTrue();
            root.TryGetProperty("pageSize", out _).Should().BeTrue();
            root.TryGetProperty("totalPages", out _).Should().BeTrue();
        }

        // =========================================================
        // M-8: Sort order — unread messages appear before read messages
        // =========================================================

        [Fact]
        public async Task GetAdminMessages_UnreadBeforeRead_SortOrderIsCorrect()
        {
            // Arrange — send several messages, then mark some as read
            using HttpClient anonClient = _fixture.CreateClient();
            string subjectReadA = $"ReadMsg-A-{Guid.NewGuid():N}";
            string subjectReadB = $"ReadMsg-B-{Guid.NewGuid():N}";
            string subjectUnread = $"UnreadMsg-{Guid.NewGuid():N}";

            await anonClient.SendMessageAsync("User A", "a@example.com", subjectReadA, "Body A.");
            await anonClient.SendMessageAsync("User B", "b@example.com", subjectReadB, "Body B.");
            await anonClient.SendMessageAsync("User C", "c@example.com", subjectUnread, "Body C.");

            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();

            // Retrieve all messages to find the ones we just created
            HttpResponseMessage listResponse = await adminClient.GetAdminMessagesAsync(pageSize: 50);
            listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument listDoc = await listResponse.ReadAsJsonDocumentAsync();
            JsonElement items = listDoc.RootElement.GetProperty("items");

            Guid? idReadA = null;
            Guid? idReadB = null;
            foreach (JsonElement item in items.EnumerateArray())
            {
                string? subj = item.GetProperty("subject").GetString();
                if (subj == subjectReadA)
                {
                    idReadA = Guid.Parse(item.GetProperty("id").GetString()!);
                }
                else if (subj == subjectReadB)
                {
                    idReadB = Guid.Parse(item.GetProperty("id").GetString()!);
                }
            }

            idReadA.Should().NotBeNull("setup: readA message must be findable");
            idReadB.Should().NotBeNull("setup: readB message must be findable");

            // Mark readA and readB as read
            await adminClient.PatchMessageIsReadAsync(idReadA!.Value, isRead: true);
            await adminClient.PatchMessageIsReadAsync(idReadB!.Value, isRead: true);

            // Act — re-fetch the sorted list
            HttpResponseMessage sortedResponse = await adminClient.GetAdminMessagesAsync(pageSize: 50);
            sortedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonDocument sortedDoc = await sortedResponse.ReadAsJsonDocumentAsync();
            JsonElement sortedItems = sortedDoc.RootElement.GetProperty("items");

            // Assert — verify that all unread messages appear before any read message
            bool seenReadItem = false;
            foreach (JsonElement item in sortedItems.EnumerateArray())
            {
                bool isRead = item.GetProperty("isRead").GetBoolean();
                if (isRead)
                {
                    seenReadItem = true;
                }
                else
                {
                    // An unread item after a read item means sort is wrong
                    seenReadItem.Should().BeFalse(
                        because: "all unread messages must appear before any read message in the sorted list");
                }
            }
        }

        // =========================================================
        // M-9: PATCH set isRead:true → 200, isRead=true
        // =========================================================

        [Fact]
        public async Task PatchMessage_SetIsReadTrue_Returns200AndIsReadTrue()
        {
            // Arrange — send a message and find its id
            string subject = $"PatchM9-{Guid.NewGuid():N}";
            using HttpClient anonClient = _fixture.CreateClient();
            await anonClient.SendMessageAsync("Patcher M9", "m9@example.com", subject, "Body M9.");

            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            HttpResponseMessage listResponse = await adminClient.GetAdminMessagesAsync(pageSize: 50);
            JsonDocument listDoc = await listResponse.ReadAsJsonDocumentAsync();

            Guid? messageId = null;
            foreach (JsonElement item in listDoc.RootElement.GetProperty("items").EnumerateArray())
            {
                if (item.GetProperty("subject").GetString() == subject)
                {
                    messageId = Guid.Parse(item.GetProperty("id").GetString()!);
                    break;
                }
            }

            messageId.Should().NotBeNull("setup: message must be findable in admin list");

            // Act
            HttpResponseMessage response = await adminClient.PatchMessageIsReadAsync(
                messageId!.Value, isRead: true);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            doc.RootElement.GetProperty("isRead").GetBoolean().Should().BeTrue(
                because: "PATCH must set isRead to the explicitly provided value");
        }

        // =========================================================
        // M-10: PATCH set isRead:false (revert) → 200, isRead=false
        // =========================================================

        [Fact]
        public async Task PatchMessage_SetIsReadFalseAfterRead_Returns200AndIsReadFalse()
        {
            // Arrange — send a message, mark it read, then revert to unread
            string subject = $"PatchM10-{Guid.NewGuid():N}";
            using HttpClient anonClient = _fixture.CreateClient();
            await anonClient.SendMessageAsync("Patcher M10", "m10@example.com", subject, "Body M10.");

            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            HttpResponseMessage listResponse = await adminClient.GetAdminMessagesAsync(pageSize: 50);
            JsonDocument listDoc = await listResponse.ReadAsJsonDocumentAsync();

            Guid? messageId = null;
            foreach (JsonElement item in listDoc.RootElement.GetProperty("items").EnumerateArray())
            {
                if (item.GetProperty("subject").GetString() == subject)
                {
                    messageId = Guid.Parse(item.GetProperty("id").GetString()!);
                    break;
                }
            }

            messageId.Should().NotBeNull();
            await adminClient.PatchMessageIsReadAsync(messageId!.Value, isRead: true);

            // Act — revert to unread
            HttpResponseMessage response = await adminClient.PatchMessageIsReadAsync(
                messageId.Value, isRead: false);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonDocument doc = await response.ReadAsJsonDocumentAsync();
            doc.RootElement.GetProperty("isRead").GetBoolean().Should().BeFalse(
                because: "PATCH must support explicit setting to false (revert unread)");
        }

        // =========================================================
        // M-11: PATCH non-existent messageId → 404
        // =========================================================

        [Fact]
        public async Task PatchMessage_NonExistentId_Returns404()
        {
            using HttpClient adminClient = await _fixture.CreateAdminClientAsync();
            Guid fakeId = Guid.NewGuid();

            HttpResponseMessage response = await adminClient.PatchMessageIsReadAsync(fakeId, isRead: true);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
