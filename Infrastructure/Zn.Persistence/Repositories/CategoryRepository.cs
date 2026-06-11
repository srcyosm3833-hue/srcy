using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zn.Application.Features.Categories.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;
using Zn.Persistence.Context;

namespace Zn.Persistence.Repositories
{
    /// <summary>
    /// <see cref="ICategoryRepository"/>'nin EF Core implementasyonu.
    /// Okuma sorgularında blog sayısı veritabanı seviyesinde COUNT ile projekte edilir
    /// (AsNoTracking); güncelleme/silme için yapılan okumalar tracked döner.
    /// </summary>
    public sealed class CategoryRepository : ICategoryRepository
    {
        private readonly ZnBlogDbContext _context;

        public CategoryRepository(ZnBlogDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<CategoryWithBlogCount>> GetAllWithBlogCountAsync(
            CancellationToken cancellationToken)
        {
            // Projeksiyon DB seviyesinde yapılır: Blogs koleksiyonu belleğe yüklenmez,
            // yalnızca COUNT(*) hesaplanır. Ada göre alfabetik sıralı döner.
            return await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.CategoryName)
                .Select(c => new CategoryWithBlogCount(
                    c.Id,
                    c.CategoryName,
                    c.Blogs.Count,
                    c.CreatedAt,
                    c.UpdatedAt))
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<CategoryWithBlogCount?> GetByIdWithBlogCountAsync(
            Guid id, CancellationToken cancellationToken)
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CategoryWithBlogCount(
                    c.Id,
                    c.CategoryName,
                    c.Blogs.Count,
                    c.CreatedAt,
                    c.UpdatedAt))
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            // Tracked: çağıran handler Rename/Remove uygulayıp SaveChanges ile kalıcılaştırır.
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByNameAsync(
            string categoryName, Guid? excludeId, CancellationToken cancellationToken)
        {
            // Büyük/küçük harf duyarsız karşılaştırma; SQL Server'ın varsayılan
            // collation'ı (CI) zaten case-insensitive'dir, EF.Functions.Like ile netleştiriyoruz.
            IQueryable<Category> query = _context.Categories
                .AsNoTracking()
                .Where(c => c.CategoryName == categoryName);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> HasBlogsAsync(Guid categoryId, CancellationToken cancellationToken)
        {
            return await _context.Blogs
                .AsNoTracking()
                .AnyAsync(b => b.CategoryId == categoryId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddAsync(Category category, CancellationToken cancellationToken)
        {
            await _context.Categories.AddAsync(category, cancellationToken);
        }

        /// <inheritdoc />
        public void Remove(Category category)
        {
            _context.Categories.Remove(category);
        }

        /// <inheritdoc />
        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
