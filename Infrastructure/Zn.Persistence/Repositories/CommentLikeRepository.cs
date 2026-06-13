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
    /// <see cref="ICommentLikeRepository"/>'nin EF Core implementasyonu. Toggle akışında mevcut beğeni
    /// tracked okunur (ekle/sil için), beğeni sayısı AsNoTracking COUNT ile hesaplanır. Composite PK
    /// (CommentId, UserId) duplicate beğeniyi DB seviyesinde engeller; eş zamanlı çift istekten
    /// kaynaklanabilecek <see cref="DbUpdateException"/> burada yakalanarak idempotent davranılır
    /// (EF bağımlılığı Application'a sızmaz).
    /// </summary>
    public sealed class CommentLikeRepository : ICommentLikeRepository
    {
        private readonly ZnBlogDbContext _context;

        public CommentLikeRepository(ZnBlogDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<bool> CommentExistsAsync(Guid commentId, CancellationToken cancellationToken)
        {
            return await _context.Comments
                .AsNoTracking()
                .AnyAsync(c => c.Id == commentId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<(bool Liked, int LikeCount)> ToggleAsync(
            Guid commentId,
            string userId,
            CancellationToken cancellationToken)
        {
            CommentLike? existing = await _context.CommentLikes
                .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId, cancellationToken);

            bool liked;

            if (existing is not null)
            {
                // Unlike: mevcut beğeniyi kaldır.
                _context.CommentLikes.Remove(existing);
                liked = false;
            }
            else
            {
                // Like: yeni beğeni ekle.
                CommentLike like = CommentLike.Create(commentId, userId);
                await _context.CommentLikes.AddAsync(like, cancellationToken);
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
                foreach (var entry in _context.ChangeTracker.Entries<CommentLike>())
                {
                    entry.State = EntityState.Detached;
                }
            }

            int likeCount = await _context.CommentLikes
                .AsNoTracking()
                .CountAsync(cl => cl.CommentId == commentId, cancellationToken);

            return (liked, likeCount);
        }
    }
}
