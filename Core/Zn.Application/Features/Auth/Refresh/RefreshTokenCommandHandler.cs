using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Zn.Application.Common.Results;
using Zn.Application.Features.Auth.Common;
using Zn.Application.Interfaces.Authentication;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Auth.Refresh
{
    /// <summary>
    /// <see cref="RefreshTokenCommand"/>'ı işleyen Wolverine handler'ı.
    /// Refresh token rotation ve replay tespitini uygular:
    /// <list type="bullet">
    /// <item>Token bulunamaz / süresi dolmuş → 401.</item>
    /// <item>Token zaten revoke edilmiş (replay) → kullanıcının TÜM aktif token'ları iptal + 401.</item>
    /// <item>Geçerli → eski token revoke + ReplacedByToken set, yeni access/refresh çifti üret.</item>
    /// </list>
    /// Tüm DB değişiklikleri tek SaveChanges ile atomik uygulanır.
    /// </summary>
    public static class RefreshTokenCommandHandler
    {
        public static async Task<Result<AuthTokensResponse>> Handle(
            RefreshTokenCommand command,
            IRefreshTokenRepository refreshTokenRepository,
            ITokenHasher tokenHasher,
            IJwtTokenService jwtTokenService,
            UserManager<User> userManager,
            IOptions<AuthTokenOptions> options,
            CancellationToken cancellationToken)
        {
            string incomingHash = tokenHasher.Hash(command.RefreshToken);

            RefreshToken? existing = await refreshTokenRepository.GetByTokenHashAsync(
                incomingHash, cancellationToken);

            if (existing is null)
            {
                return Result.Failure<AuthTokensResponse>(AuthErrors.InvalidRefreshToken);
            }

            // Replay tespiti: revoke edilmiş bir token tekrar kullanılıyor.
            // Bu token'la zincirlenen tüm oturum tehlikede sayılır → hepsini iptal et.
            if (existing.RevokedAt is not null)
            {
                await refreshTokenRepository.RevokeAllActiveForUserAsync(existing.UserId, cancellationToken);
                await refreshTokenRepository.SaveChangesAsync(cancellationToken);
                return Result.Failure<AuthTokensResponse>(AuthErrors.InvalidRefreshToken);
            }

            if (existing.IsExpired)
            {
                return Result.Failure<AuthTokensResponse>(AuthErrors.InvalidRefreshToken);
            }

            User? user = await userManager.FindByIdAsync(existing.UserId);
            if (user is null)
            {
                // Token geçerli ama kullanıcı yok (silinmiş) → güvenli tarafta kal.
                return Result.Failure<AuthTokensResponse>(AuthErrors.InvalidRefreshToken);
            }

            IList<string> roles = await userManager.GetRolesAsync(user);

            // Yeni token çifti üret.
            AccessToken accessToken = jwtTokenService.GenerateAccessToken(
                new TokenUser(user.Id, user.Email!, user.UserName!, (IReadOnlyCollection<string>)roles));

            string newPlainRefreshToken = jwtTokenService.GenerateRefreshToken();
            string newRefreshHash = tokenHasher.Hash(newPlainRefreshToken);
            DateTime newRefreshExpiresAt = DateTime.UtcNow.AddDays(options.Value.RefreshTokenDays);

            // Rotation: eski token'ı revoke et ve yeni token'a zincirle.
            existing.RevokedAt = DateTime.UtcNow;
            existing.ReplacedByToken = newRefreshHash;
            existing.UpdatedAt = DateTime.UtcNow;

            var newRefreshToken = new RefreshToken
            {
                Token = newRefreshHash,
                UserId = user.Id,
                ExpiresAt = newRefreshExpiresAt,
                CreatedAt = DateTime.UtcNow
            };

            await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
            await refreshTokenRepository.SaveChangesAsync(cancellationToken);

            var response = new AuthTokensResponse(
                accessToken.Value,
                accessToken.ExpiresAtUtc,
                newPlainRefreshToken,
                newRefreshExpiresAt);

            return Result.Success(response);
        }
    }
}
