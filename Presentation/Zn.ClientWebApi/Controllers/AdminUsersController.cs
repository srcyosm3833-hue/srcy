using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Zn.Application.Common.Pagination;
using Zn.Application.Common.Results;
using Zn.Application.Features.Users.CreateUser;
using Zn.Application.Features.Users.Common;
using Zn.Application.Features.Users.GetUserById;
using Zn.Application.Features.Users.GetUsers;
using Zn.Application.Features.Users.SoftDeleteUser;
using Zn.Application.Features.Users.UpdateUser;
using Zn.Domain.Authorization;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// Admin kullanıcı yönetimi uç noktaları (A6 yetki matrisi):
    /// <list type="bullet">
    /// <item>Listeleme (GET) → Admin + Manager.</item>
    /// <item>Tekil getirme (GET {id}) → Admin + Manager.</item>
    /// <item>Oluşturma / güncelleme / silme → yalnızca Admin.</item>
    /// </list>
    /// Yetki action seviyesinde <c>[Authorize(Roles = ...)]</c> ile uygulanır. Komut/sorgular Wolverine
    /// üzerinden handler'lara gönderilir; dönen <see cref="Result"/>'lar <see cref="ApiControllerBase"/>
    /// ile HTTP'ye eşlenir. Yazma uçlarında istek sahibi kimliği token'dan alınır (gövdeden asla).
    /// </summary>
    [Route("api/admin/users")]
    public sealed class AdminUsersController : ApiControllerBase
    {
        private readonly IMessageBus _messageBus;

        public AdminUsersController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        /// <summary>
        /// Kullanıcıları sayfalı listeler (Admin + Manager). includeDeleted=true ise soft delete
        /// edilmiş kullanıcılar da dahil edilir. Token yoksa 401, rol yetersizse 403.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Manager}")]
        [ProducesResponseType(typeof(PagedResult<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            var query = new GetUsersQuery(page, pageSize, includeDeleted);

            Result<PagedResult<UserResponse>> result =
                await _messageBus.InvokeAsync<Result<PagedResult<UserResponse>>>(query, cancellationToken);

            return HandleResult(result);
        }

        /// <summary>Tek bir kullanıcıyı Id ile getirir (Admin + Manager). Bulunamazsa 404.</summary>
        [HttpGet("{id}")]
        [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Manager}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            var query = new GetUserByIdQuery(id);

            Result<UserResponse> result =
                await _messageBus.InvokeAsync<Result<UserResponse>>(query, cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// Yeni kullanıcı oluşturur (yalnızca Admin); varsayılan "User" rolü atanır.
        /// Başarıda 201; aynı e-posta varsa 409; politika ihlalinde 400.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create(
            [FromBody] CreateUserByAdminCommand command,
            CancellationToken cancellationToken)
        {
            Result<UserResponse> result =
                await _messageBus.InvokeAsync<Result<UserResponse>>(command, cancellationToken);

            return HandleResult(result, value =>
                CreatedAtAction(
                    actionName: nameof(GetById),
                    routeValues: new { id = value.Id },
                    value: value));
        }

        /// <summary>
        /// Kullanıcının ad/soyad/görselini günceller (yalnızca Admin). Bulunamazsa 404; doğrulama
        /// hatasında 400. Route'taki id otoritatiftir; gövdedeki alanlarla birleştirilir.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            string id,
            [FromBody] UpdateUserRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateUserCommand(id, request.FirstName, request.LastName, request.ImageUrl);

            Result<UserResponse> result =
                await _messageBus.InvokeAsync<Result<UserResponse>>(command, cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// Kullanıcıyı soft delete eder (yalnızca Admin). Başarıda 204; bulunamazsa 404; admin kendi
        /// hesabını silmeye çalışırsa 400. İstek sahibi kimliği token'dan alınır.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            // İsteği yapan admin'in kimliği token'dan; kendini silme engeli handler'da uygulanır.
            string requestingUserId = GetUserId() ?? string.Empty;
            var command = new SoftDeleteUserCommand(id, requestingUserId);

            Result result =
                await _messageBus.InvokeAsync<Result>(command, cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// PUT gövdesi: kullanıcının güncellenebilir profil alanları. Id route'tan geldiği için
        /// gövdede tekrar edilmez. E-posta ve rol bu kapsamda değildir.
        /// </summary>
        /// <param name="FirstName">Kullanıcının yeni adı.</param>
        /// <param name="LastName">Kullanıcının yeni soyadı.</param>
        /// <param name="ImageUrl">Profil görseli URL'i (boş bırakılırsa varsayılan avatar atanır).</param>
        public sealed record UpdateUserRequest(string FirstName, string LastName, string? ImageUrl);
    }
}
