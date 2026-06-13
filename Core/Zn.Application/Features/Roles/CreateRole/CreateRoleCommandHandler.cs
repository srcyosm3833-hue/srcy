using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Zn.Application.Common.Results;
using Zn.Application.Features.Roles.Common;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Roles.CreateRole
{
    /// <summary>
    /// <see cref="CreateRoleCommand"/>'ı işleyen Wolverine handler'ı. Aynı adda rol varsa 409;
    /// başarıda yeni rolün <see cref="RoleResponse"/>'unu (UserCount=0) döner. İş mantığı incedir:
    /// ad doğrulaması validator'da, benzersizlik kontrolü RoleManager üzerinden burada yapılır.
    /// Yetki (yalnızca Admin) controller'da sağlanır.
    /// </summary>
    public static class CreateRoleCommandHandler
    {
        public static async Task<Result<RoleResponse>> Handle(
            CreateRoleCommand command,
            RoleManager<Role> roleManager,
            CancellationToken cancellationToken)
        {
            string name = command.Name.Trim();

            // Benzersizlik erken kontrolü: anlamlı 409 döndürmek için.
            if (await roleManager.RoleExistsAsync(name))
            {
                return Result.Failure<RoleResponse>(RoleErrors.Conflict(name));
            }

            var role = new Role { Name = name };

            IdentityResult createResult = await roleManager.CreateAsync(role);
            if (!createResult.Succeeded)
            {
                if (createResult.Errors.Any(e => e.Code.Contains("Duplicate")))
                {
                    return Result.Failure<RoleResponse>(RoleErrors.Conflict(name));
                }

                IReadOnlyDictionary<string, string[]> validations = MapIdentityErrors(createResult.Errors);
                return Result.Failure<RoleResponse>(RoleErrors.IdentityFailure(validations));
            }

            // Yeni rolde henüz kullanıcı yoktur; özel rol korumalı değildir.
            RoleResponse response = RoleMapper.ToResponse(role, userCount: 0, isProtected: false);
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
