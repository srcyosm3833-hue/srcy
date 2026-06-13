using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Pagination;
using Zn.Application.Common.Results;
using Zn.Application.Features.SearchLogs.Common;
using Zn.Application.Interfaces.Persistence;

namespace Zn.Application.Features.SearchLogs.GetAll
{
    /// <summary>
    /// <see cref="GetSearchLogsQuery"/>'i işleyen Wolverine handler'ı. Sayfalama parametrelerini
    /// güvenli aralığa çeker, repository'den DB seviyesinde projekte edilmiş ve SearchedAt azalan
    /// sıralı sayfayı (opsiyonel terim filtresiyle) alır ve <see cref="PagedResult{T}"/> olarak
    /// sarar. Repository doğrudan <see cref="SearchLogResponse"/>'a projekte ettiği için ayrıca
    /// mapper'a gerek yoktur. Liste boş olabilir (her zaman Success). Yetki (Admin-only) controller
    /// seviyesinde uygulanır.
    /// </summary>
    public static class GetSearchLogsQueryHandler
    {
        public static async Task<Result<PagedResult<SearchLogResponse>>> Handle(
            GetSearchLogsQuery query,
            ISearchLogRepository searchLogRepository,
            CancellationToken cancellationToken)
        {
            // Sayfalama parametrelerini güvenli aralığa normalize et.
            int page = query.Page < 1 ? 1 : query.Page;
            int pageSize = query.PageSize switch
            {
                < 1 => GetSearchLogsQuery.DefaultPageSize,
                > GetSearchLogsQuery.MaxPageSize => GetSearchLogsQuery.MaxPageSize,
                _ => query.PageSize
            };

            (IReadOnlyList<SearchLogResponse> items, int totalCount) =
                await searchLogRepository.GetPagedAsync(page, pageSize, query.Term, cancellationToken);

            var pagedResult = new PagedResult<SearchLogResponse>(items, totalCount, page, pageSize);

            return Result.Success(pagedResult);
        }
    }
}
