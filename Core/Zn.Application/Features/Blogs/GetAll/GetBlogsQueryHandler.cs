using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Pagination;
using Zn.Application.Common.Results;
using Zn.Application.Features.Blogs.Common;
using Zn.Application.Interfaces.Persistence;

namespace Zn.Application.Features.Blogs.GetAll
{
    /// <summary>
    /// <see cref="GetBlogsQuery"/>'i işleyen Wolverine handler'ı. Sayfalama parametrelerini
    /// güvenli aralığa çeker, repository'den DB seviyesinde projekte edilmiş sayfayı alır ve
    /// Mapperly ile yanıta dönüştürüp <see cref="PagedResult{T}"/> olarak sarar.
    /// Liste boş olabilir (her zaman Success).
    /// </summary>
    public static class GetBlogsQueryHandler
    {
        public static async Task<Result<PagedResult<BlogListItemResponse>>> Handle(
            GetBlogsQuery query,
            IBlogRepository blogRepository,
            CancellationToken cancellationToken)
        {
            // Sayfalama parametrelerini güvenli aralığa normalize et: aşırı büyük sayfa
            // boyutu isteklerini üst sınıra sabitle, geçersiz değerleri varsayılana çek.
            int page = query.Page < 1 ? 1 : query.Page;
            int pageSize = query.PageSize switch
            {
                < 1 => GetBlogsQuery.DefaultPageSize,
                > GetBlogsQuery.MaxPageSize => GetBlogsQuery.MaxPageSize,
                _ => query.PageSize
            };

            // Public liste: silinmiş bloglar gösterilmez (includeDeleted=false). Admin'in
            // silinmişleri görmesi için includeDeleted'i query'ye taşıyan dilim Özellik 6'da eklenir.
            (IReadOnlyList<BlogListItem> items, int totalCount) =
                await blogRepository.GetPagedAsync(
                    page, pageSize, query.CategoryId, false, query.CurrentUserId, cancellationToken);

            IReadOnlyList<BlogListItemResponse> mapped = BlogMapper.ToListItemResponseList(items);

            var pagedResult = new PagedResult<BlogListItemResponse>(mapped, totalCount, page, pageSize);

            return Result.Success(pagedResult);
        }
    }
}
