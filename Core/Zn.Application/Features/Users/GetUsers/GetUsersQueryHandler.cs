using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Pagination;
using Zn.Application.Common.Results;
using Zn.Application.Features.Users.Common;
using Zn.Application.Interfaces.Persistence;

namespace Zn.Application.Features.Users.GetUsers
{
    /// <summary>
    /// <see cref="GetUsersQuery"/>'i işleyen Wolverine handler'ı. Sayfalama parametrelerini güvenli
    /// aralığa çeker, repository'den DB seviyesinde sıralanmış (kayıt tarihi azalan) ve projekte edilmiş
    /// sayfayı (rolleriyle birlikte) alır, Mapperly ile yanıta dönüştürüp <see cref="PagedResult{T}"/>
    /// olarak sarar. Liste boş olabilir. includeDeleted true ise silinmiş kullanıcılar da dahil edilir.
    /// </summary>
    public static class GetUsersQueryHandler
    {
        public static async Task<Result<PagedResult<UserResponse>>> Handle(
            GetUsersQuery query,
            IUserRepository userRepository,
            CancellationToken cancellationToken)
        {
            int page = query.Page < 1 ? 1 : query.Page;
            int pageSize = query.PageSize switch
            {
                < 1 => GetUsersQuery.DefaultPageSize,
                > GetUsersQuery.MaxPageSize => GetUsersQuery.MaxPageSize,
                _ => query.PageSize
            };

            (IReadOnlyList<UserListItem> items, int totalCount) =
                await userRepository.GetPagedAsync(page, pageSize, query.IncludeDeleted, cancellationToken);

            IReadOnlyList<UserResponse> mapped = UserMapper.ToResponseList(items);

            var pagedResult = new PagedResult<UserResponse>(mapped, totalCount, page, pageSize);

            return Result.Success(pagedResult);
        }
    }
}
