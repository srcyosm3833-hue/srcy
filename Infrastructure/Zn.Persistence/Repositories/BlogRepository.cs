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
            bool includeDeleted,
            string? currentUserId,
            CancellationToken cancellationToken)
        {
            IQueryable<Blog> query = _context.Blogs.AsNoTracking();

            // Admin/Manager sorgusu: soft delete edilmiş blogları da görmek için global query
            // filter bypass edilir. Public sorguda (false) silinmiş bloglar zaten dışlanır.
            if (includeDeleted)
            {
                query = query.IgnoreQueryFilters();
            }

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
                    b.CreatedAt,
                    // Beğeni sayısı ve "bu kullanıcı beğendi mi" DB seviyesinde (COUNT / EXISTS)
                    // hesaplanır; like koleksiyonu belleğe çekilmez (N+1 yok).
                    b.Likes.Count,
                    currentUserId != null && b.Likes.Any(l => l.UserId == currentUserId)))
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <inheritdoc />
        public async Task<(IReadOnlyList<BlogListItem> Items, int TotalCount)> SearchAsync(
            string q,
            Guid? categoryId,
            int page,
            int pageSize,
            string? currentUserId,
            CancellationToken cancellationToken)
        {
            // Soft delete edilmiş bloglar global query filter ile zaten dışlanır (bypass YOK).
            IQueryable<Blog> query = _context.Blogs.AsNoTracking();

            if (categoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }

            // LIKE deseni: terim başlık VEYA açıklamada herhangi bir yerde geçmeli (içeren eşleşme).
            // EF.Functions.Like SQL tarafında çalışır; '%' wildcard'larını sarmalıyoruz.
            string pattern = $"%{q}%";
            query = query.Where(b =>
                EF.Functions.Like(b.Title, pattern) ||
                EF.Functions.Like(b.Description, pattern));

            // Toplam sayı filtreye (kategori + arama) göre sayfalamadan önce hesaplanır.
            int totalCount = await query.CountAsync(cancellationToken);

            // En yeni bloglar önce. Projeksiyon DB seviyesinde: yalnızca liste için
            // gereken kolonlar + yazar adı birleştirmesi SQL tarafında yapılır (GetPaged ile tutarlı).
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
                    b.CreatedAt,
                    // Beğeni sayısı ve "bu kullanıcı beğendi mi" DB seviyesinde (COUNT / EXISTS)
                    // hesaplanır; GetPaged ile tutarlı.
                    b.Likes.Count,
                    currentUserId != null && b.Likes.Any(l => l.UserId == currentUserId)))
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <inheritdoc />
        public async Task<BlogDetail?> GetDetailByIdAsync(Guid id, string? currentUserId, CancellationToken cancellationToken)
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
                    b.UpdatedAt,
                    // Beğeni sayısı ve "bu kullanıcı beğendi mi" DB seviyesinde hesaplanır.
                    b.Likes.Count,
                    currentUserId != null && b.Likes.Any(l => l.UserId == currentUserId)))
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
        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
