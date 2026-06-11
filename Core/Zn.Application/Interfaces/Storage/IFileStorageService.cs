using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;

namespace Zn.Application.Interfaces.Storage
{
    /// <summary>
    /// Dosya (görsel) depolama soyutlaması. İmplementasyon Zn.Infrastructure'dadır
    /// (geliştirme için yerel diske yazan <c>LocalFileStorageService</c>).
    /// <para>
    /// Production'da bu arayüz, aynı sözleşmeyle bir bulut depolama (Azure Blob Storage / S3)
    /// implementasyonuyla değiştirilebilir; çağıran kod (UploadImage handler/endpoint) değişmez.
    /// </para>
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Bir görsel dosyasını doğrulayıp depolar ve erişilebilir göreli URL'ini döner
        /// (örn. "/uploads/{benzersiz-ad}.jpg"). Doğrulama (boş/boyut/uzantı) implementasyon
        /// içinde yapılır; ihlalde anlamlı bir <see cref="Error"/> ile başarısız sonuç döner.
        /// </summary>
        Task<Result<string>> SaveImageAsync(FileUploadRequest request, CancellationToken cancellationToken);
    }
}
