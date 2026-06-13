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
            // Yorumlar ve alt yorumlar ortak düz moderasyon şekline projekte edilir. EF Core, set
            // operasyonunu (Concat/UNION ALL) yalnızca her iki taraf AYNI anonim tipe projekte
            // edildiğinde SQL'e çevirebilir; constructor'lı record (CommentModerationItem) projeksiyonu
            // istemci-projeksiyonu sayılıp set operasyonunu çeviremez. Bu yüzden önce anonim tipe
            // projekte edilir, birleştirilir, sıralama + sayfalama DB'de yapılır; yalnız nihai sayfa
            // (≤ pageSize) bellekte record'a maplenir. Global query filter'lar (silinmiş blog/kullanıcı)
            // navigation üzerinden otomatik uygulanmaya devam eder.
            var comments = _context.Comments
                .AsNoTracking()
                .Select(c => new
                {
                    c.Id,
                    IsReply = false,
                    c.BlogId,
                    BlogTitle = c.Blog.Title,
                    c.UserId,
                    AuthorName = c.User.FirstName + " " + c.User.LastName,
                    Text = c.CommentText,
                    c.CreatedAt,
                    ParentCommentId = (Guid?)null,
                });

            var replies = _context.SubComments
                .AsNoTracking()
                .Select(s => new
                {
                    s.Id,
                    IsReply = true,
                    BlogId = s.Comment.BlogId,
                    BlogTitle = s.Comment.Blog.Title,
                    s.UserId,
                    AuthorName = s.User.FirstName + " " + s.User.LastName,
                    Text = s.SubCommentText,
                    s.CreatedAt,
                    ParentCommentId = (Guid?)s.CommentId,
                });

            var combined = comments.Concat(replies);

            int totalCount = await combined.CountAsync(cancellationToken);

            var rows = await combined
                .OrderByDescending(i => i.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            List<CommentModerationItem> items = rows
                .Select(r => new CommentModerationItem(
                    r.Id,
                    r.IsReply,
                    r.BlogId,
                    r.BlogTitle,
                    r.UserId,
                    r.AuthorName,
                    r.Text,
                    r.CreatedAt,
                    r.ParentCommentId))
                .ToList();

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
