using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Zn.Application.Common.Results;
using Zn.Application.Features.Roles.Common;
using Zn.Domain.Authorization;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Roles.GetRoles
{
    /// <summary>
    /// <see cref="GetRolesQuery"/>'i işleyen Wolverine handler'ı. Tüm rolleri ada göre sıralı çeker;
    /// her rol için <see cref="UserManager{TUser}.GetUsersInRoleAsync"/> ile atanmış kullanıcı sayısını
    /// hesaplar ve <see cref="RoleResponse"/>'a eşler. Korumalı roller (Admin/Manager/User)
    /// <c>IsProtected=true</c> ile işaretlenir; frontend buna göre Düzenle/Sil butonlarını kilitler.
    /// Yetki (yalnızca Admin) controller'da sağlanır.
    /// </summary>
    public static class GetRolesQueryHandler
    {
        public static async Task<Result<IReadOnlyList<RoleResponse>>> Handle(
            GetRolesQuery query,
            RoleManager<Role> roleManager,
            UserManager<User> userManager,
            CancellationToken cancellationToken)
        {
            // Application katmanı EF Core'a bağımlı değildir; bu yüzden ToListAsync yerine senkron
            // materializasyon kullanılır. Rol sayısı sınırlı olduğundan (sistemde birkaç rol) bu kabul
            // edilebilir; her rol için ayrıca async GetUsersInRoleAsync çağrılır.
            List<Role> roles = roleManager.Roles
                .OrderBy(r => r.Name)
                .ToList();

            var responses = new List<RoleResponse>(roles.Count);
            foreach (Role role in roles)
            {
                // GetUsersInRoleAsync CancellationToken kabul etmez; rol sayısı sınırlı olduğundan kabul edilir.
                IList<User> usersInRole = await userManager.GetUsersInRoleAsync(role.Name!);
                bool isProtected = RoleNames.IsProtected(role.Name!);

                responses.Add(RoleMapper.ToResponse(role, usersInRole.Count, isProtected));
            }

            return Result.Success<IReadOnlyList<RoleResponse>>(responses);
        }
    }
}
