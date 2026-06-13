using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Zn.Application.Common.Results;
using Zn.Application.Features.Roles.Common;
using Zn.Application.Features.Roles.CreateRole;
using Zn.Application.Features.Roles.DeleteRole;
using Zn.Application.Features.Roles.GetRoles;
using Zn.Application.Features.Roles.UpdateRole;
using Zn.Domain.Authorization;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// Admin rol yönetimi (CRUD) uç noktaları. Tümü <b>yalnızca Admin</b> yetkisindedir (A6 matrisi:
    /// rol yönetimi yalnız Admin). Komut/sorgular Wolverine üzerinden handler'lara gönderilir; dönen
    /// <see cref="Result"/>'lar <see cref="ApiControllerBase"/> ile HTTP'ye eşlenir.
    /// <para>
    /// Korumalı roller (Admin/Manager/User) güncellenemez/silinemez (400); aynı adda rol oluşturma 409;
    /// kullanıcısı olan rol silme 409; bulunamayan rol 404.
    /// </para>
    /// </summary>
    [Route("api/admin/roles")]
    [Authorize(Roles = RoleNames.Admin)]
    public sealed class AdminRolesController : ApiControllerBase
    {
        private readonly IMessageBus _messageBus;

        public AdminRolesController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        /// <summary>
        /// Tüm rolleri, her rol için atanmış kullanıcı sayısıyla listeler. Korumalı roller
        /// <c>isProtected=true</c> ile işaretlenir. Token yoksa 401, Admin değilse 403.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<RoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
        {
            var query = new GetRolesQuery();

            Result<IReadOnlyList<RoleResponse>> result =
                await _messageBus.InvokeAsync<Result<IReadOnlyList<RoleResponse>>>(query, cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// Yeni özel rol oluşturur. Başarıda 201 + rol temsili; aynı ad varsa 409; doğrulama hatasında 400.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create(
            [FromBody] CreateRoleCommand command,
            CancellationToken cancellationToken)
        {
            Result<RoleResponse> result =
                await _messageBus.InvokeAsync<Result<RoleResponse>>(command, cancellationToken);

            return HandleResult(result, value => Created($"/api/admin/roles/{value.Id}", value));
        }

        /// <summary>
        /// Bir rolü yeniden adlandırır (route'taki id otoritatiftir). Korumalı rol 400; bulunamazsa 404;
        /// yeni ad çakışırsa 409. Başarıda 200 + güncel rol temsili.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(
            string id,
            [FromBody] UpdateRoleRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateRoleCommand(id, request.Name);

            Result<RoleResponse> result =
                await _messageBus.InvokeAsync<Result<RoleResponse>>(command, cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// Bir rolü siler. Korumalı rol 400; bulunamazsa 404; role atanmış kullanıcı varsa 409.
        /// Başarıda 204.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            var command = new DeleteRoleCommand(id);

            Result result =
                await _messageBus.InvokeAsync<Result>(command, cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// PUT gövdesi: rolün yeni adı. Id route'tan geldiği için gövdede tekrar edilmez.
        /// </summary>
        /// <param name="Name">Rolün yeni adı.</param>
        public sealed record UpdateRoleRequest(string Name);
    }
}
