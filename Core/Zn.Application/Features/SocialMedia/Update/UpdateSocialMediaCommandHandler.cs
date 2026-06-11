using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.SocialMedia.Common;
using Zn.Application.Interfaces.Persistence;
using DomainSocialMedia = Zn.Domain.Entity.SocialMedia;

namespace Zn.Application.Features.SocialMedia.Update
{
    /// <summary>
    /// <see cref="UpdateSocialMediaCommand"/>'ı işleyen Wolverine handler'ı.
    /// Kayıt yoksa NotFound (404). Alan değişikliği <see cref="DomainSocialMedia.Update"/> ile
    /// invariant korunarak yapılır; UpdatedAt orada güncellenir.
    /// </summary>
    public static class UpdateSocialMediaCommandHandler
    {
        public static async Task<Result<SocialMediaResponse>> Handle(
            UpdateSocialMediaCommand command,
            ISocialMediaRepository socialMediaRepository,
            CancellationToken cancellationToken)
        {
            DomainSocialMedia? socialMedia =
                await socialMediaRepository.GetByIdAsync(command.Id, cancellationToken);

            if (socialMedia is null)
            {
                return Result.Failure<SocialMediaResponse>(SocialMediaErrors.NotFound(command.Id));
            }

            // Başka bir kaydın başlığına çekmek unique index'i ihlal eder; anlamlı 409 için
            // önce kontrol ederiz (kaydın kendisi excludeId ile hariç tutulur). 404 → 409 sırası.
            bool titleTaken = await socialMediaRepository.ExistsByTitleAsync(
                command.Title, excludeId: command.Id, cancellationToken);

            if (titleTaken)
            {
                return Result.Failure<SocialMediaResponse>(
                    SocialMediaErrors.TitleAlreadyExists(command.Title));
            }

            // Invariant'lar Domain mutator'ında korunur (boş değil, azami uzunluk, UpdatedAt).
            socialMedia.Update(command.Title, command.Url, command.Icon);

            await socialMediaRepository.SaveChangesAsync(cancellationToken);

            SocialMediaResponse response = SocialMediaMapper.ToResponse(socialMedia);

            return Result.Success(response);
        }
    }
}
