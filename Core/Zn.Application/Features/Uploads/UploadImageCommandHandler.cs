using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Interfaces.Storage;

namespace Zn.Application.Features.Uploads
{
    /// <summary>
    /// <see cref="UploadImageCommand"/>'ı işleyen Wolverine handler'ı. Doğrulama (boş/boyut/uzantı)
    /// ve depolama <see cref="IFileStorageService"/> implementasyonunda yapılır; handler yalnızca
    /// dönen URL'i yanıt DTO'suna sarar. İhlalde storage servisi anlamlı bir hata döndürür.
    /// </summary>
    public static class UploadImageCommandHandler
    {
        public static async Task<Result<UploadImageResponse>> Handle(
            UploadImageCommand command,
            IFileStorageService fileStorageService,
            CancellationToken cancellationToken)
        {
            Result<string> stored =
                await fileStorageService.SaveImageAsync(command.File, cancellationToken);

            if (stored.IsFailure)
            {
                return Result.Failure<UploadImageResponse>(stored.Error);
            }

            return Result.Success(new UploadImageResponse(stored.Value));
        }
    }
}
