using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Zn.Application.Common.Results;
using Zn.Application.Features.Roles.Common;
using Zn.Domain.Authorization;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Roles.UpdateRole
{
    /// <summary>
    /// <see cref="UpdateRoleCommand"/>'ı işleyen Wolverine handler'ı. Sıra: rol yoksa 404; mevcut adı
    /// korumalı (Admin/Manager/User) ise 400; yeni ad da korumalı bir ad ise 400; yeni ad başka bir
    /// rolde kullanılıyorsa 409. Aksi halde RoleManager ile yeniden adlandırır ve güncel temsili döner.
    /// Yetki (yalnızca Admin) controller'da sağlanır.
    /// </summary>
    public static class UpdateRoleCommandHandler
    {
        public static async Task<Result<RoleResponse>> Handle(
            UpdateRoleCommand command,
            RoleManager<Role> roleManager,
            UserManager<User> userManager,
            CancellationToken cancellationToken)
        {
            Role? role = await roleManager.FindByIdAsync(command.RoleId);
            if (role is null)
            {
                return Result.Failure<RoleResponse>(RoleErrors.NotFound(command.RoleId));
            }

            // Korumalı rolün adı değiştirilemez (Admin/Manager/User sistemce sabittir).
            if (RoleNames.IsProtected(role.Name!))
            {
                return Result.Failure<RoleResponse>(RoleErrors.ProtectedRole(role.Name!));
            }

            string newName = command.Name.Trim();

            // Yeni ad da korumalı bir ada dönüştürülemez (özel rol "Admin" olamaz).
            if (RoleNames.IsProtected(newName))
            {
                return Result.Failure<RoleResponse>(RoleErrors.ProtectedRole(newName));
            }

            // Ad gerçekten değişiyorsa benzersizlik kontrolü yap (kendisi hariç).
            if (!string.Equals(role.Name, newName, StringComparison.OrdinalIgnoreCase))
            {
                Role? existing = await roleManager.FindByNameAsync(newName);
                if (existing is not null && existing.Id != role.Id)
                {
                    return Result.Failure<RoleResponse>(RoleErrors.Conflict(newName));
                }
            }

            role.Name = newName;

            IdentityResult updateResult = await roleManager.UpdateAsync(role);
            if (!updateResult.Succeeded)
            {
                if (updateResult.Errors.Any(e => e.Code.Contains("Duplicate")))
                {
                    return Result.Failure<RoleResponse>(RoleErrors.Conflict(newName));
                }

                IReadOnlyDictionary<string, string[]> validations = MapIdentityErrors(updateResult.Errors);
                return Result.Failure<RoleResponse>(RoleErrors.IdentityFailure(validations));
            }

            IList<User> usersInRole = await userManager.GetUsersInRoleAsync(role.Name!);
            RoleResponse response = RoleMapper.ToResponse(role, usersInRole.Count, isProtected: false);
            return Result.Success(response);
        }

        /// <summary>Identity hatalarını ProblemDetails "errors" sözlüğüne uygun yapıya çevirir.</summary>
        private static IReadOnlyDictionary<string, string[]> MapIdentityErrors(
            IEnumerable<IdentityError> errors)
        {
            return new Dictionary<string, string[]>
            {
                ["Role"] = errors.Select(e => e.Description).ToArray()
            };
        }
    }
}
