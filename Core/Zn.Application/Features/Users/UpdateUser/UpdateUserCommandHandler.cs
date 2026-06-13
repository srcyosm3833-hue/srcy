using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Zn.Application.Common.Results;
using Zn.Application.Features.Users.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Users.UpdateUser
{
    /// <summary>
    /// <see cref="UpdateUserCommand"/>'ı işleyen Wolverine handler'ı. Kullanıcıyı UserManager üzerinden
    /// bulup ad/soyad/görsel alanlarını günceller. Soft delete edilmiş kullanıcılar global query filter
    /// nedeniyle UserManager tarafından görülemez → bulunamazsa 404 (silinmiş kullanıcı güncellenemez).
    /// Yetki (yalnızca Admin) controller'da <c>[Authorize(Roles = "Admin")]</c> ile sağlanır.
    /// </summary>
    public static class UpdateUserCommandHandler
    {
        /// <summary>Görsel verilmediğinde atanan varsayılan avatar (Register/seed ile aynı değer).</summary>
        private const string DefaultImageUrl = "https://www.gravatar.com/avatar/?d=mp";

        public static async Task<Result<UserResponse>> Handle(
            UpdateUserCommand command,
            UserManager<User> userManager,
            IUserRepository userRepository,
            CancellationToken cancellationToken)
        {
            User? user = await userManager.FindByIdAsync(command.UserId);
            if (user is null)
            {
                // Kayıt yok (veya soft delete edilmiş ve filtre nedeniyle görünmüyor) → 404.
                return Result.Failure<UserResponse>(UserErrors.NotFound(command.UserId));
            }

            user.FirstName = command.FirstName;
            user.LastName = command.LastName;
            user.ImageUrl = string.IsNullOrWhiteSpace(command.ImageUrl)
                ? DefaultImageUrl
                : command.ImageUrl;

            IdentityResult result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                IReadOnlyDictionary<string, string[]> validations = MapIdentityErrors(result.Errors);
                return Result.Failure<UserResponse>(UserErrors.IdentityFailure(validations));
            }

            // Güncel kaydı rolleriyle birlikte projekte edip döndür.
            UserListItem? updated = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
            if (updated is null)
            {
                // Teorik olarak ulaşılmaz (az önce güncellendi); savunma amaçlı 404.
                return Result.Failure<UserResponse>(UserErrors.NotFound(command.UserId));
            }

            return Result.Success(UserMapper.ToResponse(updated));
        }

        /// <summary>Identity hatalarını ProblemDetails "errors" sözlüğüne uygun yapıya çevirir.</summary>
        private static IReadOnlyDictionary<string, string[]> MapIdentityErrors(
            IEnumerable<IdentityError> errors)
        {
            return errors
                .GroupBy(e => "User")
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(e => e.Description).ToArray());
        }
    }
}
