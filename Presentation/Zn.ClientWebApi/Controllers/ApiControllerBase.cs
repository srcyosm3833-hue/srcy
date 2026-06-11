using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Zn.Application.Common.Results;
using Zn.Domain.Authorization;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// Tüm API controller'larının türediği temel sınıf. Application katmanının
    /// <see cref="Result"/>/<see cref="Result{TValue}"/> dönüşlerini tek noktada
    /// HTTP yanıtlarına çevirir; böylece her action'da if-else tekrarı olmaz.
    /// <para>
    /// ErrorType → HTTP eşlemesi: Validation → 400, Unauthorized → 401,
    /// Forbidden → 403, NotFound → 404, Conflict → 409, Locked → 423, Failure → 500.
    /// Başarısızlıklar RFC 7807 ProblemDetails olarak döner.
    /// </para>
    /// </summary>
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>HTTP 423 Locked, ProblemDetails'in doğrudan tanımadığı bir koddur.</summary>
        private const int Status423Locked = 423;

        /// <summary>
        /// Değer taşımayan bir <see cref="Result"/>'ı HTTP yanıtına çevirir.
        /// Başarıda verilen <paramref name="onSuccess"/> sonucu döner (varsayılan 204).
        /// </summary>
        protected IActionResult HandleResult(Result result, IActionResult? onSuccess = null)
        {
            if (result.IsSuccess)
            {
                return onSuccess ?? NoContent();
            }

            return Problem(result.Error);
        }

        /// <summary>
        /// Değer taşıyan bir <see cref="Result{TValue}"/>'ı HTTP yanıtına çevirir.
        /// Başarıda varsayılan olarak 200 + değer döner; özel başarı yanıtı için
        /// <paramref name="onSuccess"/> verilebilir (örn. 201 Created).
        /// </summary>
        protected IActionResult HandleResult<TValue>(
            Result<TValue> result,
            System.Func<TValue, IActionResult>? onSuccess = null)
        {
            if (result.IsSuccess)
            {
                return onSuccess is not null ? onSuccess(result.Value) : Ok(result.Value);
            }

            return Problem(result.Error);
        }

        /// <summary>
        /// Geçerli access token'daki kullanıcı kimliğini (sub / NameIdentifier) döner.
        /// [Authorize] ile korunan action'larda token doğrulanmış olduğundan dolu beklenir;
        /// bulunamazsa null döner (çağıran defansif kontrol edebilir).
        /// </summary>
        protected string? GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

        /// <summary>Geçerli kullanıcının Admin rolünde olup olmadığını döner.</summary>
        protected bool IsAdmin() => User.IsInRole(RoleNames.Admin);

        /// <summary>Bir <see cref="Error"/>'ü uygun durum kodlu ProblemDetails'e çevirir.</summary>
        private IActionResult Problem(Error error)
        {
            int statusCode = error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Locked => Status423Locked,
                _ => StatusCodes.Status500InternalServerError
            };

            // Alan bazlı validation detayları varsa ValidationProblemDetails döndür.
            if (error.Type == ErrorType.Validation && error.Validations is { Count: > 0 })
            {
                var validationProblem = new ValidationProblemDetails(
                    error.Validations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
                {
                    Status = statusCode,
                    Title = error.Message
                };

                return new ObjectResult(validationProblem) { StatusCode = statusCode };
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = error.Message,
                Type = $"https://httpstatuses.io/{statusCode}",
                Extensions = { ["code"] = error.Code }
            };

            return new ObjectResult(problemDetails) { StatusCode = statusCode };
        }
    }
}
