using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zn.Application.Features.Comments.Common;
using Zn.Application.Features.Comments.GetAllForAdmin;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;
using Zn.Persistence.Context;

namespace Zn.Persistence.Repositories
{
    /// <summary>
    /// <see cref="ICommentRepository"/>'nin EF Core implementasyonu. Okuma sorguları AsNoTracking +
    /// veritabanı seviyesinde projeksiyon kullanır; alt yorum sayısı (SubCommentCount) navigation
    /// koleksiyonu belleğe çekilmeden COUNT ile hesaplanır. Yazar görünen adı (FirstName + " " +
    /// LastName) projeksiyonda SQL tarafında birleştirilir. Güncelleme/silme için yapılan okumalar
    /// tracked döner.
    /// </summary>
    public sealed class CommentRepository : ICommentRepository
    {
        private readonly ZnBlogDbContext _context;

        public CommentRepository(ZnBlogDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<(IReadOnlyList<CommentListItem> Items, int TotalCount)> GetPagedByBlogIdAsync(
            Guid blogId,
            int page,
            int pageSize,
            string? currentUserId,
            CancellationToken cancellationToken)
        {
            IQueryable<Comment> query = _context.Comments
                .AsNoTracking()
                .Where(c => c.BlogId == blogId);

            int totalCount = await query.CountAsync(cancellationToken);

            // En yeni yorumlar önce. Projeksiyon DB seviyesinde: alt yorum sayısı COUNT ile,
            // yazar adı SQL tarafında birleştirilir; SubComments koleksiyonu belleğe çekilmez.
            List<CommentListItem> items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CommentListItem(
                    c.Id,
                    c.CommentText,
                    c.UserId,
                    c.User.FirstName + " " + c.User.LastName,
                    c.CreatedAt,
                    c.UpdatedAt,
                    c.SubComments.Count,
                    // Beğeni sayısı ve "bu kullanıcı beğendi mi" DB seviyesinde (COUNT / EXISTS)
                    // hesaplanır; like koleksiyonu belleğe çekilmez (N+1 yok).
                    c.Likes.Count,
                    currentUserId != null && c.Likes.Any(l => l.UserId == currentUserId)))
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <inheritdoc />
        public async Task<(IReadOnlyList<CommentModerationItem> Items, int TotalCount)> GetPagedForModerationAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            // Yorumlar ortak düz moderasyon şekline projekte edilir (üst düzey: IsReply=false,
            // ParentCommentId=null). Blog başlığı/yazar adı navigation üzerinden SQL tarafında çekilir.
            // Comment global query filter'ı (silinmiş blogun yorumları dışlanır) otomatik uygulanır.
            IQueryable<CommentModerationItem> comments = _context.Comments
                .AsNoTracking()
                .Select(c => new CommentModerationItem(
                    c.Id,
                    false,
                    c.BlogId,
                    c.Blog.Title,
                    c.UserId,
                    c.User.FirstName + " " + c.User.LastName,
                    c.CommentText,
                    c.CreatedAt,
                    (Guid?)null));

            // Alt yorumlar aynı şekle projekte edilir (IsReply=true, ParentCommentId=CommentId).
            // BlogId/BlogTitle ana yorumun blogundan türetilir. SubComment global query filter'ı
            // (silinmiş kullanıcının ya da silinmiş blogun alt yorumları dışlanır) otomatik uygulanır.
            IQueryable<CommentModerationItem> replies = _context.SubComments
                .AsNoTracking()
                .Select(s => new CommentModerationItem(
                    s.Id,
                    true,
                    s.Comment.BlogId,
                    s.Comment.Blog.Title,
                    s.UserId,
                    s.User.FirstName + " " + s.User.LastName,
                    s.SubCommentText,
                    s.CreatedAt,
                    (Guid?)s.CommentId));

            // İki küme veritabanı seviyesinde birleştirilir; sıralama + sayfalama da DB tarafında.
            IQueryable<CommentModerationItem> combined = comments.Concat(replies);

            int totalCount = await combined.CountAsync(cancellationToken);

            List<CommentModerationItem> items = await combined
                .OrderByDescending(i => i.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <inheritdoc />
        public async Task<bool> BlogExistsAsync(Guid blogId, CancellationToken cancellationToken)
        {
            return await _context.Blogs
                .AsNoTracking()
                .AnyAsync(b => b.Id == blogId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<CommentListItem?> GetResponseByIdAsync(Guid id, string? currentUserId, CancellationToken cancellationToken)
        {
            return await _context.Comments
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CommentListItem(
                    c.Id,
                    c.CommentText,
                    c.UserId,
                    c.User.FirstName + " " + c.User.LastName,
                    c.CreatedAt,
                    c.UpdatedAt,
                    c.SubComments.Count,
                    // Beğeni sayısı ve "bu kullanıcı beğendi mi" DB seviyesinde hesaplanır.
                    c.Likes.Count,
                    currentUserId != null && c.Likes.Any(l => l.UserId == currentUserId)))
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            // Tracked: çağıran handler yetki kontrolü yapıp Update/Remove uygular ve SaveChanges'le kalıcılaştırır.
            return await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddAsync(Comment comment, CancellationToken cancellationToken)
        {
            await _context.Comments.AddAsync(comment, cancellationToken);
        }

        /// <inheritdoc />
        public void Remove(Comment comment)
        {
            // Comment → SubComment ilişkisi Cascade olduğundan alt yorumlar DB tarafından otomatik silinir.
            _context.Comments.Remove(comment);
        }

        /// <inheritdoc />
        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
