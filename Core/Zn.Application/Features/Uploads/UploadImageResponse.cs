namespace Zn.Application.Features.Uploads
{
    /// <summary>
    /// Görsel yükleme yanıtı: yüklenen dosyaya erişilebilir göreli URL.
    /// </summary>
    /// <param name="Url">Statik dosya olarak servis edilen göreli URL (örn. "/uploads/abc.jpg").</param>
    public sealed record UploadImageResponse(string Url);
}
