using System;

namespace Zn.Application.Features.SocialMedia.Common
{
    /// <summary>
    /// Sosyal medya bağlantısının API yanıtı. Entity yerine dışarıya dönen sözleşmedir.
    /// </summary>
    /// <param name="Id">Bağlantının benzersiz kimliği.</param>
    /// <param name="Title">Platform adı (Instagram, X, LinkedIn vb.).</param>
    /// <param name="Url">Profil/hesap bağlantısı.</param>
    /// <param name="Icon">İkon CSS sınıfı veya ikon dosya yolu.</param>
    public sealed record SocialMediaResponse(
        Guid Id,
        string Title,
        string Url,
        string Icon);
}
