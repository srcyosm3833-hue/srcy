using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zn.Application.Features.SubComments.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;
using Zn.Persistence.Context;

namespace Zn.Persistence.Repositories
{
    /// <summary>
    /// <see cref="ISubCommentRepository"/>'nin EF Core implementasyonu. Okuma sorguları AsNoTracking +
    /// veritabanı seviyesinde projeksiyon kullanır; yazar görünen adı (FirstName + " " + LastName)
    /// SQL tarafında birleştirilir. Güncelleme/silme için yapılan okumalar tracked döner.
    /// </summary>
    public sealed class SubCommentRepository : ISubCommentRepository
    {
        private readonly ZnBlogDbContext _context;

        public SubCommentRepository(ZnBlogDbContext context)
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
        public async Task<(IReadOnlyList<SubCommentListItem> Items, int TotalCount)> GetPagedByCommentIdAsync(
            Guid commentId,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            IQueryable<SubComment> query = _context.SubComments
                .AsNoTracking()
                .Where(s => s.CommentId == commentId);

            int totalCount = await query.CountAsync(cancellationToken);

            // En yeni alt yorumlar önce (yorum listeleme deseniyle tutarlı). Projeksiyon DB
            // seviyesinde: yazar adı SQL tarafında birleştirilir, User navigation belleğe çekilmez.
            List<SubCommentListItem> items = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SubCommentListItem(
                    s.Id,
                    s.SubCommentText,
                    s.CommentId,
                    s.UserId,
                    s.User.FirstName + " " + s.User.LastName,
                    s.CreatedAt,
                    s.UpdatedAt))
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <inheritdoc />
        public async Task<SubCommentListItem?> GetResponseByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.SubComments
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new SubCommentListItem(
                    s.Id,
                    s.SubCommentText,
                    s.CommentId,
                    s.UserId,
                    s.User.FirstName + " " + s.User.LastName,
                    s.CreatedAt,
                    s.UpdatedAt))
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<SubComment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            // Tracked: çağıran handler yetki kontrolü yapıp Update/Remove uygular ve SaveChanges'le kalıcılaştırır.
            return await _context.SubComments
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddAsync(SubComment subComment, CancellationToken cancellationToken)
        {
            await _context.SubComments.AddAsync(subComment, cancellationToken);
        }

        /// <inheritdoc />
        public void Remove(SubComment subComment)
        {
            _context.SubComments.Remove(subComment);
        }

        /// <inheritdoc />
        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
