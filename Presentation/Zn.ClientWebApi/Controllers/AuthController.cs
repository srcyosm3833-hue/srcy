using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Zn.Application.Common.Results;
using Zn.Application.Features.Auth.Common;
using Zn.Application.Features.Auth.Login;
using Zn.Application.Features.Auth.Logout;
using Zn.Application.Features.Auth.Refresh;
using Zn.Application.Features.Auth.Register;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// Kimlik doğrulama uç noktaları: kayıt, giriş, token yenileme ve çıkış.
    /// Komutlar Wolverine üzerinden (IMessageBus) handler'lara gönderilir;
    /// dönen <see cref="Result"/>'lar <see cref="ApiControllerBase"/> ile HTTP'ye eşlenir.
    /// </summary>
    [Route("api/auth")]
    public sealed class AuthController : ApiControllerBase
    {
        private readonly IMessageBus _messageBus;

        public AuthController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        /// <summary>Yeni kullanıcı kaydı. Başarıda 201 + kullanıcı Id/e-posta.</summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register(
            [FromBody] RegisterCommand command,
            CancellationToken cancellationToken)
        {
            Result<RegisterResponse> result =
                await _messageBus.InvokeAsync<Result<RegisterResponse>>(command, cancellationToken);

            return HandleResult(result, value =>
                CreatedAtAction(nameof(Register), new { id = value.Id }, value));
        }

        /// <summary>Giriş. Başarıda 200 + access/refresh token çifti.</summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthTokensResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(423)]
        public async Task<IActionResult> Login(
            [FromBody] LoginCommand command,
            CancellationToken cancellationToken)
        {
            Result<AuthTokensResponse> result =
                await _messageBus.InvokeAsync<Result<AuthTokensResponse>>(command, cancellationToken);

            return HandleResult(result);
        }

        /// <summary>Token yenileme (rotation). Başarıda 200 + yeni access/refresh çifti.</summary>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthTokensResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshTokenCommand command,
            CancellationToken cancellationToken)
        {
            Result<AuthTokensResponse> result =
                await _messageBus.InvokeAsync<Result<AuthTokensResponse>>(command, cancellationToken);

            return HandleResult(result);
        }

        /// <summary>Çıkış: verilen refresh token'ı revoke eder (idempotent). Başarıda 204.</summary>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout(
            [FromBody] LogoutCommand command,
            CancellationToken cancellationToken)
        {
            Result result = await _messageBus.InvokeAsync<Result>(command, cancellationToken);

            return HandleResult(result);
        }
    }
}
