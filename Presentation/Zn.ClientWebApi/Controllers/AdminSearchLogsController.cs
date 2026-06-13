using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Zn.Application.Common.Pagination;
using Zn.Application.Common.Results;
using Zn.Application.Features.SearchLogs.Common;
using Zn.Application.Features.SearchLogs.GetAll;
using Zn.Domain.Authorization;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// Arama audit log uç noktası (<c>api/admin/search-logs</c>). Aramaların kim/ne/ne zaman/hangi IP
    /// (hash'li) bilgisini sayfalı ve (opsiyonel) terim filtreli listeler.
    /// <para>
    /// Yetki YALNIZCA <c>Admin</c>'dedir (kişisel veri riski; Manager DEĞİL — A-AU5 kararı). Token
    /// yoksa 401, rol yetersizse 403. Arama logları KVKK kapsamında kişisel veri içerebilir; erişim
    /// bilinçli olarak en dar role sınırlandırılmıştır.
    /// </para>
    /// </summary>
    [Authorize(Roles = RoleNames.Admin)]
    [Route("api/admin/search-logs")]
    public sealed class AdminSearchLogsController : ApiControllerBase
    {
        private readonly IMessageBus _messageBus;

        public AdminSearchLogsController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        /// <summary>
        /// Arama loglarını SearchedAt azalan (en yeni önce) sıralı, sayfalı döner. <paramref name="term"/>
        /// verilirse yalnızca terimi içeren kayıtlar döner (büyük/küçük harf duyarsız). pageSize üst
        /// sınırı handler'da uygulanır. Başarıda 200.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<SearchLogResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? term = null,
            CancellationToken cancellationToken = default)
        {
            Result<PagedResult<SearchLogResponse>> result =
                await _messageBus.InvokeAsync<Result<PagedResult<SearchLogResponse>>>(
                    new GetSearchLogsQuery(page, pageSize, term), cancellationToken);

            return HandleResult(result);
        }
    }
}
