using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.SocialMedia.Common;
using Zn.Application.Interfaces.Persistence;

namespace Zn.Application.Features.SocialMedia.GetAll
{
    /// <summary>
    /// <see cref="GetSocialMediaQuery"/>'i işleyen Wolverine handler'ı (plain metot konvansiyonu).
    /// Repository, alanları veritabanı seviyesinde projekte eder (AsNoTracking); handler yalnızca
    /// Mapperly ile DTO'ya çevirip döner. Liste boş olabilir (her zaman Success — 404 değil).
    /// </summary>
    public static class GetSocialMediaQueryHandler
    {
        public static async Task<Result<IReadOnlyList<SocialMediaResponse>>> Handle(
            GetSocialMediaQuery query,
            ISocialMediaRepository socialMediaRepository,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<SocialMediaListItem> items =
                await socialMediaRepository.GetAllAsync(cancellationToken);

            IReadOnlyList<SocialMediaResponse> response = SocialMediaMapper.ToResponseList(items);

            return Result.Success(response);
        }
    }
}
