using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Pagination;
using Zn.Application.Common.Results;
using Zn.Application.Features.Messages.Common;
using Zn.Application.Interfaces.Persistence;

namespace Zn.Application.Features.Messages.GetAll
{
    /// <summary>
    /// <see cref="GetMessagesQuery"/>'i işleyen Wolverine handler'ı. Sayfalama parametrelerini
    /// güvenli aralığa çeker, repository'den DB seviyesinde sıralanmış (okunmamışlar önce, sonra
    /// CreatedAt azalan) ve projekte edilmiş sayfayı alır, Mapperly ile yanıta dönüştürüp
    /// <see cref="PagedResult{T}"/> olarak sarar. Liste boş olabilir.
    /// </summary>
    public static class GetMessagesQueryHandler
    {
        public static async Task<Result<PagedResult<MessageResponse>>> Handle(
            GetMessagesQuery query,
            IMessageRepository messageRepository,
            CancellationToken cancellationToken)
        {
            // Sayfalama parametrelerini güvenli aralığa normalize et.
            int page = query.Page < 1 ? 1 : query.Page;
            int pageSize = query.PageSize switch
            {
                < 1 => GetMessagesQuery.DefaultPageSize,
                > GetMessagesQuery.MaxPageSize => GetMessagesQuery.MaxPageSize,
                _ => query.PageSize
            };

            (IReadOnlyList<MessageListItem> items, int totalCount) =
                await messageRepository.GetPagedAsync(page, pageSize, cancellationToken);

            IReadOnlyList<MessageResponse> mapped = MessageMapper.ToResponseList(items);

            var pagedResult = new PagedResult<MessageResponse>(mapped, totalCount, page, pageSize);

            return Result.Success(pagedResult);
        }
    }
}
