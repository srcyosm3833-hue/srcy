using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Zn.Application.Common.Pagination;
using Zn.Application.Common.Results;
using Zn.Application.Features.Comments.Add;
using Zn.Application.Features.Comments.Common;
using Zn.Application.Features.Comments.Delete;
using Zn.Application.Features.Comments.GetByBlogId;
using Zn.Application.Features.Comments.Update;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// Bir bloga ait yorum uç noktaları (<c>api/blogs/{blogId}/comments</c>). Yorum listeleme
    /// herkese açıktır; ekleme/düzenleme/silme kimlik doğrulama gerektirir.
    /// <para>
    /// Yetki modeli: ekleme giriş yapmış herkese açık; düzenleme yalnızca yorumun sahibine
    /// (Admin bile başkasının yorumunu düzenleyemez); silme sahibe veya Admin'e açık. Yazar/istek
    /// sahibi kimliği ve Admin rolü token'dan okunur — istek gövdesinden ASLA alınmaz.
    /// </para>
    /// </summary>
    [Route("api/blogs/{blogId:guid}/comments")]
    public sealed class CommentsController : ApiControllerBase
    {
        private readonly IMessageBus _messageBus;

        public CommentsController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        /// <summary>
        /// Bloga ait yorumları sayfalı (createdAt azalan) döner. Herkese açıktır. Blog yoksa 404.
        /// pageSize üst sınırı handler'da uygulanır.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<CommentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByBlogId(
            Guid blogId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            Result<PagedResult<CommentResponse>> result =
                await _messageBus.InvokeAsync<Result<PagedResult<CommentResponse>>>(
                    new GetCommentsByBlogIdQuery(blogId, page, pageSize), cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// Bloga yorum ekler. Giriş yapmış her kullanıcı yorum yapabilir; yazar token'dan alınır.
        /// Başarıda 201; blog yoksa 404; token yoksa 401.
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Add(
            Guid blogId,
            [FromBody] AddCommentRequest request,
            CancellationToken cancellationToken)
        {
            // Yazar token'dan; gövdedeki hiçbir UserId alanı kabul edilmez.
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var command = new AddCommentCommand(blogId, request.CommentText, userId);

            Result<CommentResponse> result =
                await _messageBus.InvokeAsync<Result<CommentResponse>>(command, cancellationToken);

            return HandleResult(result, value =>
                CreatedAtAction(nameof(GetByBlogId), new { blogId }, value));
        }

        /// <summary>
        /// Yorumu günceller. Yalnızca yorumun sahibi yapabilir (Admin bile başkasınınkini değil →
        /// 403). Bulunamazsa 404; token yoksa 401. Başarıda 200 (isEdited: true).
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            Guid blogId,
            Guid id,
            [FromBody] UpdateCommentRequest request,
            CancellationToken cancellationToken)
        {
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var command = new UpdateCommentCommand(id, request.CommentText, userId);

            Result<CommentResponse> result =
                await _messageBus.InvokeAsync<Result<CommentResponse>>(command, cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// Yorumu siler. Yorumun sahibi veya Admin yapabilir (aksi halde 403). Bulunamazsa 404;
        /// token yoksa 401. Başarıda 204. Alt yorumlar veritabanı tarafından (Cascade) silinir.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(
            Guid blogId,
            Guid id,
            CancellationToken cancellationToken)
        {
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            Result result = await _messageBus.InvokeAsync<Result>(
                new DeleteCommentCommand(id, userId, IsAdmin()), cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// POST gövdesi: yorum metni. UserId bilinçli olarak yoktur — yazar token'dan alınır.
        /// </summary>
        public sealed record AddCommentRequest(string CommentText);

        /// <summary>PUT gövdesi: güncellenecek yorum metni (Id route'tan gelir).</summary>
        public sealed record UpdateCommentRequest(string CommentText);
    }
}
