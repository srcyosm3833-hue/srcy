using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zn.Application.Features.SocialMedia.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Persistence.Context;
using DomainSocialMedia = Zn.Domain.Entity.SocialMedia;

namespace Zn.Persistence.Repositories
{
    /// <summary>
    /// <see cref="ISocialMediaRepository"/>'nin EF Core implementasyonu.
    /// Liste sorgusu DB seviyesinde projekte eder (AsNoTracking); güncelleme/silme için
    /// yapılan tekil okuma tracked entity döner.
    /// </summary>
    public sealed class SocialMediaRepository : ISocialMediaRepository
    {
        private readonly ZnBlogDbContext _context;

        public SocialMediaRepository(ZnBlogDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<SocialMediaListItem>> GetAllAsync(CancellationToken cancellationToken)
        {
            // Projeksiyon DB seviyesinde yapılır: entity belleğe çekilmez, yalnızca
            // gereken alanlar seçilir. Başlığa göre alfabetik sıralı döner.
            return await _context.SocialMedias
                .AsNoTracking()
                .OrderBy(s => s.Title)
                .Select(s => new SocialMediaListItem(
                    s.Id,
                    s.Title,
                    s.Url,
                    s.Icon))
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<DomainSocialMedia?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            // Tracked: çağıran handler Update/Remove uygulayıp SaveChanges ile kalıcılaştırır.
            return await _context.SocialMedias
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByTitleAsync(
            string title, Guid? excludeId, CancellationToken cancellationToken)
        {
            // SQL Server'ın varsayılan collation'ı (CI) zaten case-insensitive'dir.
            IQueryable<DomainSocialMedia> query = _context.SocialMedias
                .AsNoTracking()
                .Where(s => s.Title == title);

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddAsync(DomainSocialMedia socialMedia, CancellationToken cancellationToken)
        {
            await _context.SocialMedias.AddAsync(socialMedia, cancellationToken);
        }

        /// <inheritdoc />
        public void Remove(DomainSocialMedia socialMedia)
        {
            _context.SocialMedias.Remove(socialMedia);
        }

        /// <inheritdoc />
        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
