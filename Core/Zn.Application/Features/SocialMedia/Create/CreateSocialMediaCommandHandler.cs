using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.SocialMedia.Common;
using Zn.Application.Interfaces.Persistence;
using DomainSocialMedia = Zn.Domain.Entity.SocialMedia;

namespace Zn.Application.Features.SocialMedia.Create
{
    /// <summary>
    /// <see cref="CreateSocialMediaCommand"/>'ı işleyen Wolverine handler'ı (plain metot konvansiyonu).
    /// İş mantığı incedir: invariant'lar (boş değil, azami uzunluk) <see cref="DomainSocialMedia.Create"/>
    /// factory'sinde korunur; handler yalnızca oluşturup kaydeder ve yanıtı Mapperly ile döner.
    /// </summary>
    public static class CreateSocialMediaCommandHandler
    {
        public static async Task<Result<SocialMediaResponse>> Handle(
            CreateSocialMediaCommand command,
            ISocialMediaRepository socialMediaRepository,
            CancellationToken cancellationToken)
        {
            // Uygulama seviyesinde erken duplicate kontrolü: Title üzerindeki DB unique index'i
            // son savunma hattıdır, ama anlamlı 409 döndürmek için önce burada kontrol ederiz.
            bool exists = await socialMediaRepository.ExistsByTitleAsync(
                command.Title, excludeId: null, cancellationToken);

            if (exists)
            {
                return Result.Failure<SocialMediaResponse>(
                    SocialMediaErrors.TitleAlreadyExists(command.Title));
            }

            // Invariant'lar Domain factory'sinde korunur (boş değil, azami uzunluk).
            DomainSocialMedia socialMedia = DomainSocialMedia.Create(
                command.Title, command.Url, command.Icon);

            await socialMediaRepository.AddAsync(socialMedia, cancellationToken);
            await socialMediaRepository.SaveChangesAsync(cancellationToken);

            SocialMediaResponse response = SocialMediaMapper.ToResponse(socialMedia);

            return Result.Success(response);
        }
    }
}
