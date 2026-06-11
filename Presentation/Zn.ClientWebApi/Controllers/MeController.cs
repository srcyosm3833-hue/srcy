using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// Geçerli access token'a sahip kullanıcının kimlik bilgilerini döndüren
    /// korumalı uç nokta. JWT Bearer altyapısının uçtan uca çalıştığını doğrular.
    /// </summary>
    [Authorize]
    [Route("api/me")]
    public sealed class MeController : ApiControllerBase
    {
        /// <summary>
        /// Token'daki kullanıcı Id, e-posta, kullanıcı adı ve rolleri döner.
        /// Token yoksa/geçersizse JWT Bearer middleware 401 döndürür (buraya gelinmez).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Get()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? User.FindFirstValue("sub");
            string? email = User.FindFirstValue(ClaimTypes.Email)
                            ?? User.FindFirstValue("email");
            string? userName = User.FindFirstValue(ClaimTypes.Name);
            IReadOnlyCollection<string> roles = User.FindAll(ClaimTypes.Role)
                                                     .Select(c => c.Value)
                                                     .ToArray();

            var response = new CurrentUserResponse(userId, email, userName, roles);
            return Ok(response);
        }

        /// <summary>/api/me yanıtı: token'dan okunan kullanıcı kimlik bilgileri.</summary>
        public sealed record CurrentUserResponse(
            string? Id,
            string? Email,
            string? UserName,
            IReadOnlyCollection<string> Roles);
    }
}
