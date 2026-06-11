using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Zn.Application.Common.Results;
using Zn.Application.Features.Contact.Common;
using Zn.Application.Features.Contact.Upsert;
using Zn.Domain.Authorization;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// İletişim bilgisinin yönetici (Admin) uç noktası (<c>api/admin/contact</c>). Tekil iletişim
    /// kaydını upsert eder (PUT): yoksa oluşturur (201), varsa günceller (200). İkinci bir kayıt asla
    /// oluşmaz. Action <c>[Authorize(Roles = "Admin")]</c> ile korunur — token yoksa 401, rol
    /// yetersizse 403.
    /// </summary>
    [Authorize(Roles = RoleNames.Admin)]
    [Route("api/admin/contact")]
    public sealed class AdminContactController : ApiControllerBase
    {
        private readonly IMessageBus _messageBus;

        public AdminContactController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        /// <summary>
        /// İletişim bilgisini upsert eder. Kayıt yoksa oluşturulur (201), varsa güncellenir (200).
        /// Doğrulama hatasında 400.
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Upsert(
            [FromBody] UpsertContactCommand command,
            CancellationToken cancellationToken)
        {
            Result<UpsertContactResult> result =
                await _messageBus.InvokeAsync<Result<UpsertContactResult>>(command, cancellationToken);

            // WasCreated → 201 (Created), aksi halde 200 (güncellendi). Her iki durumda da gövde
            // güncel iletişim kaydını taşır.
            return HandleResult(result, value => value.WasCreated
                ? StatusCode(StatusCodes.Status201Created, value.Contact)
                : Ok(value.Contact));
        }
    }
}
