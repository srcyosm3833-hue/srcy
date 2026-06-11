namespace Zn.Application.Features.SocialMedia.Create
{
    /// <summary>
    /// Yeni sosyal medya bağlantısı oluşturma komutu (admin). Başarıda oluşturulan kaydın
    /// <see cref="Common.SocialMediaResponse"/>'u döner. Immutable record.
    /// </summary>
    /// <param name="Title">Platform adı (Instagram, X, LinkedIn vb.).</param>
    /// <param name="Url">Profil/hesap bağlantısı (geçerli mutlak URL).</param>
    /// <param name="Icon">İkon CSS sınıfı veya ikon dosya yolu.</param>
    public sealed record CreateSocialMediaCommand(string Title, string Url, string Icon);
}
