using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Pagination;
using Zn.Application.Common.Results;
using Zn.Application.Features.Comments.Common;
using Zn.Application.Interfaces.Persistence;

namespace Zn.Application.Features.Comments.GetByBlogId
{
    /// <summary>
    /// <see cref="GetCommentsByBlogIdQuery"/>'i işleyen Wolverine handler'ı. Önce blog varlığını
    /// doğrular (yoksa 404), ardından sayfalama parametrelerini güvenli aralığa çeker, repository'den
    /// DB seviyesinde projekte edilmiş (alt yorum sayısı COUNT ile) sayfayı alır ve Mapperly ile
    /// yanıta dönüştürüp <see cref="PagedResult{T}"/> olarak sarar. Liste boş olabilir.
    /// </summary>
    public static class GetCommentsByBlogIdQueryHandler
    {
        public static async Task<Result<PagedResult<CommentResponse>>> Handle(
            GetCommentsByBlogIdQuery query,
            ICommentRepository commentRepository,
            CancellationToken cancellationToken)
        {
            // Var olmayan blog için boş liste yerine anlamlı 404 döndür.
            bool blogExists = await commentRepository.BlogExistsAsync(query.BlogId, cancellationToken);
            if (!blogExists)
            {
                return Result.Failure<PagedResult<CommentResponse>>(
                    CommentErrors.BlogNotFound(query.BlogId));
            }

            // Sayfalama parametrelerini güvenli aralığa normalize et.
            int page = query.Page < 1 ? 1 : query.Page;
            int pageSize = query.PageSize switch
            {
                < 1 => GetCommentsByBlogIdQuery.DefaultPageSize,
                > GetCommentsByBlogIdQuery.MaxPageSize => GetCommentsByBlogIdQuery.MaxPageSize,
                _ => query.PageSize
            };

            (IReadOnlyList<CommentListItem> items, int totalCount) =
                await commentRepository.GetPagedByBlogIdAsync(query.BlogId, page, pageSize, cancellationToken);

            IReadOnlyList<CommentResponse> mapped = CommentMapper.ToResponseList(items);

            var pagedResult = new PagedResult<CommentResponse>(mapped, totalCount, page, pageSize);

            return Result.Success(pagedResult);
        }
    }
}
