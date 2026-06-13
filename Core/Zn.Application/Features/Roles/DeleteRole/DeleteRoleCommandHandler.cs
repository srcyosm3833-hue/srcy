using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Zn.Application.Common.Results;
using Zn.Application.Features.Roles.Common;
using Zn.Domain.Authorization;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Roles.DeleteRole
{
    /// <summary>
    /// <see cref="DeleteRoleCommand"/>'ı işleyen Wolverine handler'ı. Sıra: rol yoksa 404; korumalı
    /// (Admin/Manager/User) ise 400; role atanmış aktif kullanıcı varsa 409 (kullanıcı önce bilinçli
    /// olarak rolden çıkarılmalı). Aksi halde RoleManager ile siler ve değer taşımayan başarı döner.
    /// Yetki (yalnızca Admin) controller'da sağlanır.
    /// </summary>
    public static class DeleteRoleCommandHandler
    {
        public static async Task<Result> Handle(
            DeleteRoleCommand command,
            RoleManager<Role> roleManager,
            UserManager<User> userManager,
            CancellationToken cancellationToken)
        {
            Role? role = await roleManager.FindByIdAsync(command.RoleId);
            if (role is null)
            {
                return Result.Failure(RoleErrors.NotFound(command.RoleId));
            }

            // Korumalı roller silinemez (sistemin erişimsiz kalmasını önler).
            if (RoleNames.IsProtected(role.Name!))
            {
                return Result.Failure(RoleErrors.ProtectedRole(role.Name!));
            }

            // Role atanmış kullanıcı varsa silmeyi engelle; kullanıcıyı bilinçli aksiyona zorla (A-AU8).
            IList<User> usersInRole = await userManager.GetUsersInRoleAsync(role.Name!);
            if (usersInRole.Count > 0)
            {
                return Result.Failure(RoleErrors.RoleHasUsers(role.Name!));
            }

            IdentityResult deleteResult = await roleManager.DeleteAsync(role);
            if (!deleteResult.Succeeded)
            {
                IReadOnlyDictionary<string, string[]> validations = new Dictionary<string, string[]>
                {
                    ["Role"] = deleteResult.Errors.Select(e => e.Description).ToArray()
                };
                return Result.Failure(RoleErrors.IdentityFailure(validations));
            }

            return Result.Success();
        }
    }
}
