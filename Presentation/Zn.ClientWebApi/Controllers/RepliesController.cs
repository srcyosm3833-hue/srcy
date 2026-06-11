using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Zn.Application.Common.Results;
using Zn.Application.Features.SubComments.Add;
using Zn.Application.Features.SubComments.Common;
using Zn.Application.Features.SubComments.Delete;
using Zn.Application.Features.SubComments.Update;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// Bir yoruma verilen yanıt (alt yorum) uç noktaları (<c>api/comments/{commentId}/replies</c>).
    /// Tüm işlemler kimlik doğrulama gerektirir.
    /// <para>
    /// Yetki modeli (yorumlarla birebir aynı): ekleme giriş yapmış herkese açık; düzenleme yalnızca
    /// alt yorumun sahibine (Admin bile başkasınınkini değil); silme sahibe veya Admin'e açık.
    /// Yazar/istek sahibi kimliği ve Admin rolü token'dan okunur — istek gövdesinden ASLA alınmaz.
    /// </para>
    /// </summary>
    [Route("api/comments/{commentId:guid}/replies")]
    public sealed class RepliesController : ApiControllerBase
    {
        private readonly IMessageBus _messageBus;

        public RepliesController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        /// <summary>
        /// Yoruma yanıt (alt yorum) ekler. Giriş yapmış her kullanıcı yanıt verebilir; yazar
        /// token'dan alınır. Başarıda 201; ana yorum yoksa 404; token yoksa 401.
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(SubCommentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Add(
            Guid commentId,
            [FromBody] AddReplyRequest request,
            CancellationToken cancellationToken)
        {
            // Yazar token'dan; gövdedeki hiçbir UserId alanı kabul edilmez.
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var command = new AddSubCommentCommand(commentId, request.SubCommentText, userId);

            Result<SubCommentResponse> result =
                await _messageBus.InvokeAsync<Result<SubCommentResponse>>(command, cancellationToken);

            return HandleResult(result, value =>
                CreatedAtAction(nameof(Add), new { commentId }, value));
        }

        /// <summary>
        /// Alt yorumu günceller. Yalnızca alt yorumun sahibi yapabilir (Admin bile başkasınınkini
        /// değil → 403). Bulunamazsa 404; token yoksa 401. Başarıda 200 (isEdited: true).
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(SubCommentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            Guid commentId,
            Guid id,
            [FromBody] UpdateReplyRequest request,
            CancellationToken cancellationToken)
        {
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var command = new UpdateSubCommentCommand(id, request.SubCommentText, userId);

            Result<SubCommentResponse> result =
                await _messageBus.InvokeAsync<Result<SubCommentResponse>>(command, cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// Alt yorumu siler. Alt yorumun sahibi veya Admin yapabilir (aksi halde 403). Bulunamazsa
        /// 404; token yoksa 401. Başarıda 204.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(
            Guid commentId,
            Guid id,
            CancellationToken cancellationToken)
        {
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            Result result = await _messageBus.InvokeAsync<Result>(
                new DeleteSubCommentCommand(id, userId, IsAdmin()), cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// POST gövdesi: alt yorum metni. UserId bilinçli olarak yoktur — yazar token'dan alınır.
        /// </summary>
        public sealed record AddReplyRequest(string SubCommentText);

        /// <summary>PUT gövdesi: güncellenecek alt yorum metni (Id route'tan gelir).</summary>
        public sealed record UpdateReplyRequest(string SubCommentText);
    }
}
