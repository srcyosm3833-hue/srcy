using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Pagination;
using Zn.Application.Common.Results;
using Zn.Application.Features.SubComments.Common;
using Zn.Application.Interfaces.Persistence;

namespace Zn.Application.Features.SubComments.GetByCommentId
{
    /// <summary>
    /// <see cref="GetRepliesByCommentIdQuery"/>'i işleyen Wolverine handler'ı. Önce ana yorum
    /// varlığını doğrular (yoksa 404), ardından sayfalama parametrelerini güvenli aralığa çeker,
    /// repository'den DB seviyesinde projekte edilmiş sayfayı alır ve Mapperly ile yanıta
    /// dönüştürüp <see cref="PagedResult{T}"/> olarak sarar. Liste boş olabilir.
    /// </summary>
    public static class GetRepliesByCommentIdQueryHandler
    {
        public static async Task<Result<PagedResult<SubCommentResponse>>> Handle(
            GetRepliesByCommentIdQuery query,
            ISubCommentRepository subCommentRepository,
            CancellationToken cancellationToken)
        {
            // Var olmayan ana yorum için boş liste yerine anlamlı 404 döndür.
            bool commentExists = await subCommentRepository.CommentExistsAsync(query.CommentId, cancellationToken);
            if (!commentExists)
            {
                return Result.Failure<PagedResult<SubCommentResponse>>(
                    SubCommentErrors.CommentNotFound(query.CommentId));
            }

            // Sayfalama parametrelerini güvenli aralığa normalize et.
            int page = query.Page < 1 ? 1 : query.Page;
            int pageSize = query.PageSize switch
            {
                < 1 => GetRepliesByCommentIdQuery.DefaultPageSize,
                > GetRepliesByCommentIdQuery.MaxPageSize => GetRepliesByCommentIdQuery.MaxPageSize,
                _ => query.PageSize
            };

            (IReadOnlyList<SubCommentListItem> items, int totalCount) =
                await subCommentRepository.GetPagedByCommentIdAsync(query.CommentId, page, pageSize, cancellationToken);

            IReadOnlyList<SubCommentResponse> mapped = SubCommentMapper.ToResponseList(items);

            var pagedResult = new PagedResult<SubCommentResponse>(mapped, totalCount, page, pageSize);

            return Result.Success(pagedResult);
        }
    }
}
