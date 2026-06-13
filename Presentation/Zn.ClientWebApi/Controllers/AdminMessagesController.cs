using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Zn.Application.Common.Pagination;
using Zn.Application.Common.Results;
using Zn.Application.Features.Messages.Common;
using Zn.Application.Features.Messages.GetAll;
using Zn.Application.Features.Messages.MarkAsRead;
using Zn.Domain.Authorization;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// Mesaj kutusunun içerik yöneticisi (Admin/Manager) uç noktaları (<c>api/admin/messages</c>):
    /// sayfalı listeleme (okunmamışlar önce) ve okunma durumunu açıkça (true/false) set etme. Tüm
    /// action'lar <c>[Authorize(Roles = "Admin,Manager")]</c> ile korunur — token yoksa 401, rol
    /// yetersizse 403 (A6 yetki matrisi: mesaj listeleme/yönetim Admin + Manager).
    /// </summary>
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Manager}")]
    [Route("api/admin/messages")]
    public sealed class AdminMessagesController : ApiControllerBase
    {
        private readonly IMessageBus _messageBus;

        public AdminMessagesController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        /// <summary>
        /// Mesajları sayfalı döner. Sıralama: önce okunmamışlar, ardından her grup içinde CreatedAt
        /// azalan. pageSize üst sınırı handler'da uygulanır. Başarıda 200.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<MessageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            Result<PagedResult<MessageResponse>> result =
                await _messageBus.InvokeAsync<Result<PagedResult<MessageResponse>>>(
                    new GetMessagesQuery(page, pageSize), cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// Mesajın okunma durumunu açıkça (true/false) set eder. Mesaj yoksa 404. Başarıda 200 +
        /// güncellenmiş mesaj döner (explicit set olduğu için güncel kayıt yanıt olarak verilir).
        /// </summary>
        [HttpPatch("{id:guid}")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsRead(
            Guid id,
            [FromBody] MarkMessageAsReadRequest request,
            CancellationToken cancellationToken)
        {
            // Route'taki id otoritatiftir; gövde yalnızca yeni okunma durumunu taşır.
            var command = new MarkMessageAsReadCommand(id, request.IsRead);

            Result<MessageResponse> result =
                await _messageBus.InvokeAsync<Result<MessageResponse>>(command, cancellationToken);

            return HandleResult(result);
        }

        /// <summary>PATCH gövdesi: yeni okunma durumu. Id route'tan geldiği için gövdede tekrar edilmez.</summary>
        /// <param name="IsRead">Yeni okunma durumu (true=okundu, false=okunmadı).</param>
        public sealed record MarkMessageAsReadRequest(bool IsRead);
    }
}
