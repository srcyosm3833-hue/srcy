using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;
using Zn.Persistence.Context;

namespace Zn.Persistence.Repositories
{
    /// <summary>
    /// <see cref="IRefreshTokenRepository"/>'nin EF Core implementasyonu.
    /// Token'ları takip ederek (tracked) döndürür; böylece rotation/revoke için
    /// alanları güncellenip tek SaveChanges ile yazılabilir.
    /// </summary>
    public sealed class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ZnBlogDbContext _context;

        public RefreshTokenRepository(ZnBlogDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
        {
            // Tracked: çağıran handler RevokedAt/ReplacedByToken alanlarını güncelleyip
            // SaveChanges ile kalıcılaştıracağı için AsNoTracking KULLANILMAZ.
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == tokenHash, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RevokeAllActiveForUserAsync(string userId, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.UtcNow;

            // Aktif (revoke edilmemiş ve süresi dolmamış) token'ları çek ve revoke et.
            List<RefreshToken> activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > now)
                .ToListAsync(cancellationToken);

            foreach (RefreshToken token in activeTokens)
            {
                token.RevokedAt = now;
                token.UpdatedAt = now;
            }
        }

        /// <inheritdoc />
        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
