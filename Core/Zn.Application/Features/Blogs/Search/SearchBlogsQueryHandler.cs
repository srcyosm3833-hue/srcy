using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Pagination;
using Zn.Application.Common.Results;
using Zn.Application.Features.Blogs.Common;
using Zn.Application.Interfaces.Persistence;

namespace Zn.Application.Features.Blogs.Search
{
    /// <summary>
    /// <see cref="SearchBlogsQuery"/>'i işleyen Wolverine handler'ı. Query doğrulaması
    /// (Q zorunlu, uzunluk + pageSize aralığı) Wolverine FluentValidation middleware'i
    /// tarafından handler'dan önce uygulanır (<see cref="SearchBlogsQueryValidator"/>).
    /// Burada page yine de güvenli alt sınıra çekilir, repository'den DB seviyesinde
    /// projekte edilmiş sayfa alınır ve Mapperly ile yanıta dönüştürülüp
    /// <see cref="PagedResult{T}"/> olarak sarılır. Sonuç boş olabilir (her zaman Success).
    /// </summary>
    public static class SearchBlogsQueryHandler
    {
        public static async Task<Result<PagedResult<BlogListItemResponse>>> Handle(
            SearchBlogsQuery query,
            IBlogRepository blogRepository,
            CancellationToken cancellationToken)
        {
            // Validator pageSize'ı 1–MaxPageSize aralığında garanti eder; page için yalnızca
            // alt sınırı (≥ 1) defansif olarak normalize ediyoruz.
            int page = query.Page < 1 ? 1 : query.Page;
            int pageSize = query.PageSize;

            // Public arama: silinmiş bloglar global query filter ile zaten dışlanır.
            (IReadOnlyList<BlogListItem> items, int totalCount) =
                await blogRepository.SearchAsync(
                    query.Q, query.CategoryId, page, pageSize, query.CurrentUserId, cancellationToken);

            IReadOnlyList<BlogListItemResponse> mapped = BlogMapper.ToListItemResponseList(items);

            var pagedResult = new PagedResult<BlogListItemResponse>(mapped, totalCount, page, pageSize);

            return Result.Success(pagedResult);
        }
    }
}
