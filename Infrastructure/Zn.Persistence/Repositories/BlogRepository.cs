using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zn.Application.Features.Blogs.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;
using Zn.Persistence.Context;

namespace Zn.Persistence.Repositories
{
    /// <summary>
    /// <see cref="IBlogRepository"/>'nin EF Core implementasyonu. Okuma sorguları AsNoTracking +
    /// veritabanı seviyesinde projeksiyon kullanır; liste sorgusunda ağır alanlar (Description,
    /// BlogImage) çekilmez. Yazar adı (FirstName + " " + LastName) projeksiyonda SQL tarafında
    /// birleştirilir. Güncelleme/silme için yapılan okumalar tracked döner.
    /// </summary>
    public sealed class BlogRepository : IBlogRepository
    {
        private readonly ZnBlogDbContext _context;

        public BlogRepository(ZnBlogDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<(IReadOnlyList<BlogListItem> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            Guid? categoryId,
            CancellationToken cancellationToken)
        {
            IQueryable<Blog> query = _context.Blogs.AsNoTracking();

            if (categoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }

            // Toplam sayı filtreye göre hesaplanır (sayfalamadan önce).
            int totalCount = await query.CountAsync(cancellationToken);

            // En yeni bloglar önce. Projeksiyon DB seviyesinde: yalnızca liste için
            // gereken kolonlar + yazar adı birleştirmesi SQL tarafında yapılır.
            List<BlogListItem> items = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BlogListItem(
                    b.Id,
                    b.Title,
                    b.CoverImage,
                    b.CategoryId,
                    b.Category.CategoryName,
                    b.User.FirstName + " " + b.User.LastName,
                    b.CreatedAt))
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <inheritdoc />
        public async Task<BlogDetail?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Blogs
                .AsNoTracking()
                .Where(b => b.Id == id)
                .Select(b => new BlogDetail(
                    b.Id,
                    b.Title,
                    b.CoverImage,
                    b.BlogImage,
                    b.Description,
                    b.CategoryId,
                    b.Category.CategoryName,
                    b.UserId,
                    b.User.FirstName + " " + b.User.LastName,
                    b.CreatedAt,
                    b.UpdatedAt))
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Blog?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            // Tracked: çağıran handler yetki kontrolü yapıp Update/Remove uygular ve SaveChanges'le kalıcılaştırır.
            return await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken)
        {
            return await _context.Categories
                .AsNoTracking()
                .AnyAsync(c => c.Id == categoryId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddAsync(Blog blog, CancellationToken cancellationToken)
        {
            await _context.Blogs.AddAsync(blog, cancellationToken);
        }

        /// <inheritdoc />
        public void Remove(Blog blog)
        {
            _context.Blogs.Remove(blog);
        }

        /// <inheritdoc />
        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
