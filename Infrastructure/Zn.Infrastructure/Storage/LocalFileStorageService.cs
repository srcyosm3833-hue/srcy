using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Zn.Application.Common.Results;
using Zn.Application.Interfaces.Storage;

namespace Zn.Infrastructure.Storage
{
    /// <summary>
    /// <see cref="IFileStorageService"/>'in geliştirme/ön-üretim için yerel diske yazan
    /// implementasyonu. Dosyaları <see cref="FileStorageOptions.RootPath"/> (genellikle
    /// wwwroot/uploads) altına benzersiz bir adla kaydeder ve erişilebilir göreli URL döner.
    /// <para>
    /// PRODUCTION NOTU: Bu sınıf bilinçli olarak yerel dosya sistemine bağımlıdır ve çok
    /// sunuculu/ölçeklenen ortamlarda uygun değildir. Production'da, aynı
    /// <see cref="IFileStorageService"/> sözleşmesini uygulayan bir bulut depolama servisi
    /// (Azure Blob Storage / Amazon S3) ile değiştirilmelidir; çağıran kod (handler/endpoint)
    /// değişmeden kalır — yalnızca DI kaydı (InfrastructureRegistration) güncellenir.
    /// </para>
    /// </summary>
    public sealed class LocalFileStorageService : IFileStorageService
    {
        private readonly FileStorageOptions _options;

        public LocalFileStorageService(IOptions<FileStorageOptions> options)
        {
            _options = options.Value;
        }

        /// <inheritdoc />
        public async Task<Result<string>> SaveImageAsync(
            FileUploadRequest request, CancellationToken cancellationToken)
        {
            // --- Doğrulama: boş, boyut, uzantı ---
            if (request.Length <= 0)
            {
                return Result.Failure<string>(StorageErrors.Empty());
            }

            if (request.Length > _options.MaxFileSizeBytes)
            {
                return Result.Failure<string>(StorageErrors.TooLarge(_options.MaxFileSizeBytes));
            }

            string extension = Path.GetExtension(request.FileName).ToLowerInvariant();
            bool extensionAllowed = _options.AllowedExtensions
                .Any(allowed => string.Equals(allowed, extension, StringComparison.OrdinalIgnoreCase));

            if (!extensionAllowed)
            {
                string allowedList = string.Join(", ", _options.AllowedExtensions);
                return Result.Failure<string>(StorageErrors.UnsupportedType(allowedList));
            }

            // --- Kaydetme: benzersiz ad ile çakışma ve orijinal ad sızıntısı önlenir ---
            Directory.CreateDirectory(_options.RootPath);

            string uniqueName = $"{Guid.NewGuid():N}{extension}";
            string absolutePath = Path.Combine(_options.RootPath, uniqueName);

            await using (FileStream target = File.Create(absolutePath))
            {
                await request.Content.CopyToAsync(target, cancellationToken);
            }

            // Statik dosya middleware'inin servis edeceği göreli URL (ileri eğik çizgili).
            string url = $"{_options.RequestPath.TrimEnd('/')}/{uniqueName}";

            return Result.Success(url);
        }
    }
}
