using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Users.Common;
using Zn.Application.Interfaces.Persistence;

namespace Zn.Application.Features.Users.GetUserById
{
    /// <summary>
    /// <see cref="GetUserByIdQuery"/>'i işleyen Wolverine handler'ı. Repository'den DB seviyesinde
    /// (filtresiz, rolleriyle birlikte) projekte edilmiş kullanıcıyı alır; kayıt yoksa
    /// <see cref="UserErrors.NotFound(string)"/> ile 404 döndürür, varsa Mapperly ile yanıta eşler.
    /// Yalnızca okuma (AsNoTracking).
    /// </summary>
    public static class GetUserByIdQueryHandler
    {
        public static async Task<Result<UserResponse>> Handle(
            GetUserByIdQuery query,
            IUserRepository userRepository,
            CancellationToken cancellationToken)
        {
            UserListItem? item = await userRepository.GetByIdAsync(query.Id, cancellationToken);

            if (item is null)
            {
                return Result.Failure<UserResponse>(UserErrors.NotFound(query.Id));
            }

            return Result.Success(UserMapper.ToResponse(item));
        }
    }
}
