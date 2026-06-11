using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Zn.Application.Common.Results;
using Zn.Application.Features.SocialMedia.Common;
using Zn.Application.Features.SocialMedia.GetAll;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// Sosyal medya bağlantılarının herkese açık okuma uç noktası (kimlik doğrulama gerekmez).
    /// Sorgu Wolverine üzerinden handler'a gönderilir; dönen <see cref="Result"/> <see cref="ApiControllerBase"/>
    /// ile HTTP'ye eşlenir. Yazma işlemleri için <see cref="AdminSocialMediaController"/>'a bakınız.
    /// </summary>
    [AllowAnonymous]
    [Route("api/social-media")]
    public sealed class SocialMediaController : ApiControllerBase
    {
        private readonly IMessageBus _messageBus;

        public SocialMediaController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        /// <summary>
        /// Tüm sosyal medya bağlantılarını döner. Kayıt yoksa boş dizi (404 değil). Başarıda 200.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<SocialMediaResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            Result<IReadOnlyList<SocialMediaResponse>> result =
                await _messageBus.InvokeAsync<Result<IReadOnlyList<SocialMediaResponse>>>(
                    new GetSocialMediaQuery(), cancellationToken);

            return HandleResult(result);
        }
    }
}
