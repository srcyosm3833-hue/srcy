using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Pagination;
using Zn.Application.Common.Results;
using Zn.Application.Interfaces.Persistence;

namespace Zn.Application.Features.Comments.GetAllForAdmin
{
    /// <summary>
    /// <see cref="GetCommentsForAdminQuery"/>'i işleyen Wolverine handler'ı. Sayfalama parametrelerini
    /// güvenli aralığa normalize eder, repository'den DB seviyesinde projekte edilmiş + birleştirilmiş
    /// (yorum + alt yorum) sayfalı moderasyon kümesini alır ve Mapperly ile yanıta dönüştürüp
    /// <see cref="PagedResult{T}"/> olarak sarar. Yetki (yalnız Admin) route seviyesinde uygulanır.
    /// </summary>
    public static class GetCommentsForAdminQueryHandler
    {
        public static async Task<Result<PagedResult<CommentModerationResponse>>> Handle(
            GetCommentsForAdminQuery query,
            ICommentRepository commentRepository,
            CancellationToken cancellationToken)
        {
            // Sayfalama parametrelerini güvenli aralığa normalize et (istemci DB'yi yoramaz).
            int page = query.Page < 1 ? 1 : query.Page;
            int pageSize = query.PageSize switch
            {
                < 1 => GetCommentsForAdminQuery.DefaultPageSize,
                > GetCommentsForAdminQuery.MaxPageSize => GetCommentsForAdminQuery.MaxPageSize,
                _ => query.PageSize
            };

            (IReadOnlyList<CommentModerationItem> items, int totalCount) =
                await commentRepository.GetPagedForModerationAsync(page, pageSize, cancellationToken);

            IReadOnlyList<CommentModerationResponse> mapped =
                CommentModerationMapper.ToResponseList(items);

            var pagedResult = new PagedResult<CommentModerationResponse>(mapped, totalCount, page, pageSize);

            return Result.Success(pagedResult);
        }
    }
}
