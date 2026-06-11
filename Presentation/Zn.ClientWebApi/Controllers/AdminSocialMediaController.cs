using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Zn.Application.Common.Results;
using Zn.Application.Features.SocialMedia.Common;
using Zn.Application.Features.SocialMedia.Create;
using Zn.Application.Features.SocialMedia.Delete;
using Zn.Application.Features.SocialMedia.Update;
using Zn.Domain.Authorization;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// Sosyal medya bağlantılarının yönetici (Admin) yazma uç noktaları: oluştur, güncelle, sil.
    /// Tüm action'lar <c>[Authorize(Roles = "Admin")]</c> ile korunur — token yoksa 401,
    /// rol yetersizse 403. Komutlar Wolverine üzerinden handler'lara gönderilir;
    /// dönen <see cref="Result"/>'lar <see cref="ApiControllerBase"/> ile HTTP'ye eşlenir.
    /// </summary>
    [Authorize(Roles = RoleNames.Admin)]
    [Route("api/admin/social-media")]
    public sealed class AdminSocialMediaController : ApiControllerBase
    {
        private readonly IMessageBus _messageBus;

        public AdminSocialMediaController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        /// <summary>Yeni sosyal medya bağlantısı oluşturur. Başarıda 201.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(SocialMediaResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(
            [FromBody] CreateSocialMediaCommand command,
            CancellationToken cancellationToken)
        {
            Result<SocialMediaResponse> result =
                await _messageBus.InvokeAsync<Result<SocialMediaResponse>>(command, cancellationToken);

            // Tekil bir GET endpoint'i bulunmadığından (yalnızca koleksiyon GET'i var)
            // Location üretmeden 201 + gövde döneriz.
            return HandleResult(result, value => StatusCode(StatusCodes.Status201Created, value));
        }

        /// <summary>Var olan bağlantıyı günceller. Bulunamazsa 404. Başarıda 200.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(SocialMediaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateSocialMediaRequest request,
            CancellationToken cancellationToken)
        {
            // Route'taki id otoritatiftir; gövdedeki alanlarla birleştirilip komut oluşturulur.
            var command = new UpdateSocialMediaCommand(id, request.Title, request.Url, request.Icon);

            Result<SocialMediaResponse> result =
                await _messageBus.InvokeAsync<Result<SocialMediaResponse>>(command, cancellationToken);

            return HandleResult(result);
        }

        /// <summary>Bağlantıyı siler. Bulunamazsa 404. Başarıda 204.</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            Result result =
                await _messageBus.InvokeAsync<Result>(new DeleteSocialMediaCommand(id), cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// PUT gövdesi: güncellenecek alanları taşır. Id route'tan geldiği için gövdede
        /// tekrar edilmez (çelişki/güvenlik riskini önler).
        /// </summary>
        /// <param name="Title">Yeni platform adı.</param>
        /// <param name="Url">Yeni profil/hesap bağlantısı.</param>
        /// <param name="Icon">Yeni ikon CSS sınıfı veya yolu.</param>
        public sealed record UpdateSocialMediaRequest(string Title, string Url, string Icon);
    }
}
