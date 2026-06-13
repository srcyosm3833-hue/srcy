using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;
using Zn.Persistence.Context;

namespace Zn.Persistence.Repositories
{
    /// <summary>
    /// <see cref="IBlogLikeRepository"/>'nin EF Core implementasyonu. Toggle akışında mevcut beğeni
    /// tracked okunur (ekle/sil için), beğeni sayısı AsNoTracking COUNT ile hesaplanır. Composite PK
    /// (BlogId, UserId) duplicate beğeniyi DB seviyesinde engeller; eş zamanlı çift istekten
    /// kaynaklanabilecek <see cref="DbUpdateException"/> burada yakalanarak idempotent davranılır
    /// (EF bağımlılığı Application'a sızmaz).
    /// </summary>
    public sealed class BlogLikeRepository : IBlogLikeRepository
    {
        private readonly ZnBlogDbContext _context;

        public BlogLikeRepository(ZnBlogDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<bool> BlogExistsAsync(Guid blogId, CancellationToken cancellationToken)
        {
            // Soft delete edilmiş bloglar global query filter ile dışlanır: silinmiş bloga
            // beğeni atılamaz (toggle handler bunu 404 olarak yorumlar).
            return await _context.Blogs
                .AsNoTracking()
                .AnyAsync(b => b.Id == blogId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<(bool Liked, int LikeCount)> ToggleAsync(
            Guid blogId,
            string userId,
            CancellationToken cancellationToken)
        {
            BlogLike? existing = await _context.BlogLikes
                .FirstOrDefaultAsync(bl => bl.BlogId == blogId && bl.UserId == userId, cancellationToken);

            bool liked;

            if (existing is not null)
            {
                // Unlike: mevcut beğeniyi kaldır.
                _context.BlogLikes.Remove(existing);
                liked = false;
            }
            else
            {
                // Like: yeni beğeni ekle.
                BlogLike like = BlogLike.Create(blogId, userId);
                await _context.BlogLikes.AddAsync(like, cancellationToken);
                liked = true;
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Eş zamanlı çift istek: like yolunda PK çakışması (zaten beğenilmiş) ya da
                // unlike yolunda kayıt başka istekçe silinmiş olabilir. Her iki durumda da
                // hedeflenen son durum (liked) zaten sağlanmıştır; değişiklikleri geri çekip
                // idempotent biçimde güncel sayımla devam ederiz.
                foreach (var entry in _context.ChangeTracker.Entries<BlogLike>())
                {
                    entry.State = EntityState.Detached;
                }
            }

            int likeCount = await _context.BlogLikes
                .AsNoTracking()
                .CountAsync(bl => bl.BlogId == blogId, cancellationToken);

            return (liked, likeCount);
        }
    }
}
