using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Zn.Application.Common.Results;
using Zn.Application.Features.Users.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Authorization;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Users.CreateUser
{
    /// <summary>
    /// <see cref="CreateUserByAdminCommand"/>'ı işleyen Wolverine handler'ı. Kullanıcıyı UserManager ile
    /// oluşturur ve varsayılan "User" rolünü atar. İş mantığı incedir: doğrulama validator'da, şifre
    /// politikası Identity'dedir. RegisterCommandHandler kalıbını izler (DefaultImageUrl, duplicate→409).
    /// Yetki (yalnızca Admin) controller'da sağlanır.
    /// </summary>
    public static class CreateUserByAdminCommandHandler
    {
        /// <summary>Görsel verilmediğinde atanan varsayılan avatar (Register/seed ile aynı değer).</summary>
        private const string DefaultImageUrl = "https://www.gravatar.com/avatar/?d=mp";

        public static async Task<Result<UserResponse>> Handle(
            CreateUserByAdminCommand command,
            UserManager<User> userManager,
            IUserRepository userRepository,
            CancellationToken cancellationToken)
        {
            // Duplicate e-posta erken kontrolü: anlamlı 409 döndürmek için.
            // FindByEmailAsync soft delete filtresine takılır; silinmiş bir e-postayla çakışma
            // CreateAsync sırasında DuplicateUserName/Email olarak yakalanır (aşağıda 409'a eşlenir).
            User? existing = await userManager.FindByEmailAsync(command.Email);
            if (existing is not null)
            {
                return Result.Failure<UserResponse>(UserErrors.EmailAlreadyExists(command.Email));
            }

            var user = new User
            {
                UserName = command.Email,
                Email = command.Email,
                FirstName = command.FirstName,
                LastName = command.LastName,
                ImageUrl = string.IsNullOrWhiteSpace(command.ImageUrl)
                    ? DefaultImageUrl
                    : command.ImageUrl
            };

            IdentityResult createResult = await userManager.CreateAsync(user, command.Password);
            if (!createResult.Succeeded)
            {
                if (createResult.Errors.Any(e => e.Code.Contains("Duplicate")))
                {
                    return Result.Failure<UserResponse>(UserErrors.EmailAlreadyExists(command.Email));
                }

                IReadOnlyDictionary<string, string[]> validations = MapIdentityErrors(createResult.Errors);
                return Result.Failure<UserResponse>(UserErrors.IdentityFailure(validations));
            }

            // Varsayılan "User" rolünü ata. Rol seed'i açılışta yapıldığından mevcut olmalıdır.
            await userManager.AddToRoleAsync(user, RoleNames.User);

            // Oluşturulan kaydı rolleriyle birlikte projekte edip döndür.
            UserListItem? created = await userRepository.GetByIdAsync(user.Id, cancellationToken);
            if (created is null)
            {
                // Teorik olarak ulaşılmaz; savunma amaçlı.
                return Result.Failure<UserResponse>(UserErrors.NotFound(user.Id));
            }

            return Result.Success(UserMapper.ToResponse(created));
        }

        /// <summary>Identity hatalarını ProblemDetails "errors" sözlüğüne uygun yapıya çevirir.</summary>
        private static IReadOnlyDictionary<string, string[]> MapIdentityErrors(
            IEnumerable<IdentityError> errors)
        {
            return errors
                .GroupBy(e => e.Code.Contains("Password") ? "Password" : "User")
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(e => e.Description).ToArray());
        }
    }
}
