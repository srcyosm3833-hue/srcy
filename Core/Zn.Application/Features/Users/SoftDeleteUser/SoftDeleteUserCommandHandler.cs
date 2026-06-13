using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Zn.Application.Common.Results;
using Zn.Application.Features.Users.Common;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Users.SoftDeleteUser
{
    /// <summary>
    /// <see cref="SoftDeleteUserCommand"/>'ı işleyen Wolverine handler'ı. Kullanıcıyı UserManager üzerinden
    /// bulup <see cref="User.SoftDelete"/> mutator'ını uygular (IsDeleted=true, DeletedAt=UtcNow) ve kaydeder.
    /// <para>
    /// Yetki sırası: önce kayıt yoksa 404 (kayıt varlığı sızdırılmaz), sonra iş kuralı —
    /// admin kendi hesabını silemez (400). Soft delete edilmiş kullanıcılar global query filter nedeniyle
    /// UserManager tarafından görülemez; daha önce silinmiş bir kullanıcı için tekrar çağrılırsa 404 döner.
    /// </para>
    /// Yetki (yalnızca Admin) controller'da <c>[Authorize(Roles = "Admin")]</c> ile sağlanır.
    /// </summary>
    public static class SoftDeleteUserCommandHandler
    {
        public static async Task<Result> Handle(
            SoftDeleteUserCommand command,
            UserManager<User> userManager,
            CancellationToken cancellationToken)
        {
            User? user = await userManager.FindByIdAsync(command.TargetUserId);
            if (user is null)
            {
                return Result.Failure(UserErrors.NotFound(command.TargetUserId));
            }

            // İş kuralı: admin kendi hesabını silemez (sistemden kendini kilitlemeyi önler).
            if (command.TargetUserId == command.RequestingUserId)
            {
                return Result.Failure(UserErrors.CannotDeleteSelf);
            }

            user.SoftDelete();

            // Identity üzerinden kalıcılaştır: SecurityStamp güncellenir, değişiklik atomik kaydedilir.
            await userManager.UpdateAsync(user);

            return Result.Success();
        }
    }
}
