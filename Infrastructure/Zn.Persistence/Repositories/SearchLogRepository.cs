using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zn.Application.Features.SearchLogs.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;
using Zn.Persistence.Context;

namespace Zn.Persistence.Repositories
{
    /// <summary>
    /// <see cref="ISearchLogRepository"/>'nin EF Core implementasyonu. Yazma kendi SaveChanges'ini
    /// kapsar (arama loglama ateşle-unut yan etkisidir). Listeleme AsNoTracking + DB seviyesinde
    /// projeksiyon + SearchedAt azalan sıralama + opsiyonel terim LIKE filtresi kullanır.
    /// </summary>
    public sealed class SearchLogRepository : ISearchLogRepository
    {
        private readonly ZnBlogDbContext _context;

        public SearchLogRepository(ZnBlogDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task AddAsync(SearchLog searchLog, CancellationToken cancellationToken)
        {
            // Log yazımı bağımsız bir yan etki olduğundan kendi kalıcılaştırmasını yapar.
            await _context.SearchLogs.AddAsync(searchLog, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<(IReadOnlyList<SearchLogResponse> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            string? term,
            CancellationToken cancellationToken)
        {
            IQueryable<SearchLog> query = _context.SearchLogs.AsNoTracking();

            // Opsiyonel terim filtresi: büyük/küçük harf duyarsız "içeren" eşleşme (LIKE %term%).
            // SQL Server'da varsayılan collation çoğunlukla case-insensitive'dir; EF.Functions.Like
            // sorguyu DB tarafında çalıştırır.
            if (!string.IsNullOrWhiteSpace(term))
            {
                string pattern = $"%{term.Trim()}%";
                query = query.Where(s => EF.Functions.Like(s.Term, pattern));
            }

            int totalCount = await query.CountAsync(cancellationToken);

            // En yeni aramalar önce; projeksiyon doğrudan response tipine (ara DTO gerekmez).
            List<SearchLogResponse> items = await query
                .OrderByDescending(s => s.SearchedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SearchLogResponse(
                    s.Id,
                    s.Term,
                    s.UserId,
                    s.UserFullName,
                    s.IpHash,
                    s.SearchedAt))
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
