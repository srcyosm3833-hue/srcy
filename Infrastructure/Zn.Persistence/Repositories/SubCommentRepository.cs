using System;
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
