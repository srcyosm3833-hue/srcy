using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Zn.Application.Common.Results;
using Zn.Application.Features.Messages.Send;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// İletişim formunun herkese açık uç noktası (<c>api/messages</c>). Anonim ziyaretçiler mesaj
    /// gönderebilir; mesaj okunmamış (IsRead=false) kaydedilir. Yanıt minimaldir: yalnızca onay
    /// döner, oluşturulan kaydın Id'si paylaşılmaz.
    /// </summary>
    [AllowAnonymous]
    [Route("api/messages")]
    public sealed class MessagesController : ApiControllerBase
    {
        private readonly IMessageBus _messageBus;

        public MessagesController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        /// <summary>
        /// İletişim formundan mesaj gönderir. Başarıda 201 (gövde yok); doğrulama hatasında 400.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Send(
            [FromBody] SendMessageCommand command,
            CancellationToken cancellationToken)
        {
            Result result = await _messageBus.InvokeAsync<Result>(command, cancellationToken);

            // Minimal yanıt: ziyaretçiye yalnızca onay; kaynak Id'si döndürülmez.
            return HandleResult(result, StatusCode(StatusCodes.Status201Created));
        }
    }
}
