using System;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Interfaces.Authentication;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Auth.Logout
{
    /// <summary>
    /// <see cref="LogoutCommand"/>'ı işleyen Wolverine handler'ı.
    /// Verilen refresh token'ı revoke eder. İdempotenttir: token yoksa ya da
    /// zaten revoke edilmişse de Success döner; istemci için sonuç değişmez.
    /// </summary>
    public static class LogoutCommandHandler
    {
        public static async Task<Result> Handle(
            LogoutCommand command,
            IRefreshTokenRepository refreshTokenRepository,
            ITokenHasher tokenHasher,
            CancellationToken cancellationToken)
        {
            string incomingHash = tokenHasher.Hash(command.RefreshToken);

            RefreshToken? existing = await refreshTokenRepository.GetByTokenHashAsync(
                incomingHash, cancellationToken);

            // İdempotent: bilinmeyen veya zaten revoke edilmiş token sessizce başarı sayılır.
            if (existing is null || existing.RevokedAt is not null)
            {
                return Result.Success();
            }

            existing.RevokedAt = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;
            await refreshTokenRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
