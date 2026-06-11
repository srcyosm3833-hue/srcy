using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.SocialMedia.Common;
using Zn.Application.Interfaces.Persistence;
using DomainSocialMedia = Zn.Domain.Entity.SocialMedia;

namespace Zn.Application.Features.SocialMedia.Delete
{
    /// <summary>
    /// <see cref="DeleteSocialMediaCommand"/>'ı işleyen Wolverine handler'ı.
    /// Kayıt yoksa NotFound (404). Bağlı navigation olmadığından ekstra çakışma kontrolü yoktur.
    /// </summary>
    public static class DeleteSocialMediaCommandHandler
    {
        public static async Task<Result> Handle(
            DeleteSocialMediaCommand command,
            ISocialMediaRepository socialMediaRepository,
            CancellationToken cancellationToken)
        {
            DomainSocialMedia? socialMedia =
                await socialMediaRepository.GetByIdAsync(command.Id, cancellationToken);

            if (socialMedia is null)
            {
                return Result.Failure(SocialMediaErrors.NotFound(command.Id));
            }

            socialMediaRepository.Remove(socialMedia);
            await socialMediaRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
