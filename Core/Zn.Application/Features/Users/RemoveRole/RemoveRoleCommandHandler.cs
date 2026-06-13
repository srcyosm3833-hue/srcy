using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Zn.Application.Common.Results;
using Zn.Application.Features.Roles.Common;
using Zn.Application.Features.Users.Common;
using Zn.Domain.Authorization;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Users.RemoveRole
{
    /// <summary>
    /// <see cref="RemoveRoleCommand"/>'ı işleyen Wolverine handler'ı. Sıra: kullanıcı yoksa 404;
    /// rol yoksa 404; Admin rolü kaldırılmak isteniyor ve sistemde tek Admin kullanıcı bu ise 400
    /// (son Admin koruması — R3). Kullanıcı zaten bu rolde değilse idempotent başarı döner. Aksi halde
    /// UserManager ile rolü kaldırır. Yetki (yalnızca Admin) controller'da sağlanır.
    /// </summary>
    public static class RemoveRoleCommandHandler
    {
        public static async Task<Result> Handle(
            RemoveRoleCommand command,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            CancellationToken cancellationToken)
        {
            User? user = await userManager.FindByIdAsync(command.UserId);
            if (user is null)
            {
                return Result.Failure(UserErrors.NotFound(command.UserId));
            }

            if (!await roleManager.RoleExistsAsync(command.RoleName))
            {
                return Result.Failure(RoleErrors.NotFound(command.RoleName));
            }

            // Kullanıcı zaten bu rolde değilse yapılacak iş yok; idempotent başarı.
            if (!await userManager.IsInRoleAsync(user, command.RoleName))
            {
                return Result.Success();
            }

            // Son Admin koruması: Admin rolü kaldırılıyorsa ve sistemde başka Admin yoksa engelle.
            if (string.Equals(command.RoleName, RoleNames.Admin, StringComparison.OrdinalIgnoreCase))
            {
                IList<User> admins = await userManager.GetUsersInRoleAsync(RoleNames.Admin);
                if (admins.Count <= 1)
                {
                    return Result.Failure(UserErrors.LastAdminCannotLoseRole);
                }
            }

            IdentityResult removeResult = await userManager.RemoveFromRoleAsync(user, command.RoleName);
            if (!removeResult.Succeeded)
            {
                IReadOnlyDictionary<string, string[]> validations = new Dictionary<string, string[]>
                {
                    ["Role"] = removeResult.Errors.Select(e => e.Description).ToArray()
                };
                return Result.Failure(UserErrors.IdentityFailure(validations));
            }

            return Result.Success();
        }
    }
}
