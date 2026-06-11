using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Zn.Application.Common.Results;
using Zn.Application.Features.Contact.Common;
using Zn.Application.Features.Contact.Get;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// İletişim bilgisinin herkese açık uç noktası (<c>api/contact</c>). Anonim ziyaretçiler tekil
    /// iletişim kaydını okuyabilir. Henüz hiç iletişim kaydı yapılandırılmadıysa (ilk kurulum öncesi)
    /// 404 döner.
    /// </summary>
    [AllowAnonymous]
    [Route("api/contact")]
    public sealed class ContactController : ApiControllerBase
    {
        private readonly IMessageBus _messageBus;

        public ContactController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        /// <summary>
        /// Tekil iletişim bilgisini döner. Başarıda 200; henüz kayıt yoksa 404.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            Result<ContactResponse> result =
                await _messageBus.InvokeAsync<Result<ContactResponse>>(
                    new GetContactQuery(), cancellationToken);

            return HandleResult(result);
        }
    }
}
