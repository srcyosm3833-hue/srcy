using Zn.Application.Interfaces.Storage;

namespace Zn.Application.Features.Uploads
{
    /// <summary>
    /// Bir görseli depolama servisine yükleme komutu (giriş yapmış kullanıcılar). Başarıda,
    /// yüklenen dosyanın erişilebilir göreli URL'ini (örn. "/uploads/{ad}.jpg") içeren bir yanıt
    /// döner; bu URL daha sonra blog create/update'te CoverImage/BlogImage olarak kullanılabilir.
    /// <para>
    /// <see cref="File"/> framework'ten bağımsız <see cref="FileUploadRequest"/> tipindedir;
    /// controller IFormFile'ı buna çevirir (Onion disiplini — Application IFormFile bilmez).
    /// </para>
    /// </summary>
    /// <param name="File">Yüklenecek dosyanın akış + meta bilgisi.</param>
    public sealed record UploadImageCommand(FileUploadRequest File);
}
