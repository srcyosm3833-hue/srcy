using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Zn.Application.Common.Results;
using Zn.Application.Features.Roles.Common;
using Zn.Application.Features.Users.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Users.AssignRole
{
    /// <summary>
    /// <see cref="AssignRoleCommand"/>'ı işleyen Wolverine handler'ı. Sıra: kullanıcı yoksa 404;
    /// rol yoksa 404 (<see cref="RoleErrors.NotFound"/>); kullanıcı zaten bu roldeyse atama yapmadan
    /// idempotent başarı döner. Aksi halde UserManager ile rolü atar. Başarıda kullanıcının güncel
    /// temsili (rolleriyle) döner. Yetki (yalnızca Admin) controller'da sağlanır.
    /// </summary>
    public static class AssignRoleCommandHandler
    {
        public static async Task<Result<UserResponse>> Handle(
            AssignRoleCommand command,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IUserRepository userRepository,
            CancellationToken cancellationToken)
        {
            User? user = await userManager.FindByIdAsync(command.UserId);
            if (user is null)
            {
                return Result.Failure<UserResponse>(UserErrors.NotFound(command.UserId));
            }

            // Rol varlık kontrolü: tanımsız role atama anlamlı 404 döndürür.
            if (!await roleManager.RoleExistsAsync(command.RoleName))
            {
                return Result.Failure<UserResponse>(RoleErrors.NotFound(command.RoleName));
            }

            // İdempotent: kullanıcı zaten bu roldeyse atama yapmadan güncel temsili döndür.
            if (!await userManager.IsInRoleAsync(user, command.RoleName))
            {
                IdentityResult addResult = await userManager.AddToRoleAsync(user, command.RoleName);
                if (!addResult.Succeeded)
                {
                    IReadOnlyDictionary<string, string[]> validations = new Dictionary<string, string[]>
                    {
                        ["Role"] = addResult.Errors.Select(e => e.Description).ToArray()
                    };
                    return Result.Failure<UserResponse>(UserErrors.IdentityFailure(validations));
                }
            }

            UserListItem? updated = await userRepository.GetByIdAsync(user.Id, cancellationToken);
            if (updated is null)
            {
                // Teorik olarak ulaşılmaz; savunma amaçlı.
                return Result.Failure<UserResponse>(UserErrors.NotFound(user.Id));
            }

            return Result.Success(UserMapper.ToResponse(updated));
        }
    }
}
