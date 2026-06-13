using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Zn.Application.Common.Pagination;
using Zn.Application.Common.Results;
using Zn.Application.Features.Comments.GetAllForAdmin;
using Zn.Domain.Authorization;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// Admin yorum moderasyonu uç noktaları (<c>api/admin/comments</c>). Tüm bloglardaki yorumları VE
    /// alt yorumları tek bir DÜZ (flat) listede gösterir; bir satırın silinmesi mevcut silme uçlarıyla
    /// yapılır (bu controller yalnızca LİSTELEME ekler, silme/yetki davranışını değiştirmez):
    /// <list type="bullet">
    /// <item>Yorum (isReply=false) → <c>DELETE /api/blogs/{blogId}/comments/{id}</c>.</item>
    /// <item>Alt yorum (isReply=true) → <c>DELETE /api/comments/{parentCommentId}/replies/{id}</c>.</item>
    /// </list>
    /// Yetki yalnızca <c>Admin</c>'dedir (A6: "tüm yorumların moderasyonu" Admin işidir; Manager yalnız
    /// kendi blogundaki yorumlarla ilgilenir, genel moderasyon listesine erişmez). Token yoksa 401, rol
    /// yetersizse 403.
    /// </summary>
    [Authorize(Roles = RoleNames.Admin)]
    [Route("api/admin/comments")]
    public sealed class AdminCommentsController : ApiControllerBase
    {
        private readonly IMessageBus _messageBus;

        public AdminCommentsController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        /// <summary>
        /// Tüm yorumları ve alt yorumları sayfalı, createdAt azalan (en yeni üstte) düz bir moderasyon
        /// listesi olarak döner. pageSize üst sınırı handler'da uygulanır. Silinmiş blogun yorumları ve
        /// silinmiş kullanıcının alt yorumları (global query filter) listeye dahil edilmez. Başarıda 200.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<CommentModerationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var query = new GetCommentsForAdminQuery(page, pageSize);

            Result<PagedResult<CommentModerationResponse>> result =
                await _messageBus.InvokeAsync<Result<PagedResult<CommentModerationResponse>>>(
                    query, cancellationToken);

            return HandleResult(result);
        }
    }
}
