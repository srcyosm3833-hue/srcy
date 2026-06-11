using System;

namespace Zn.Application.Features.SocialMedia.Update
{
    /// <summary>
    /// Var olan bir sosyal medya bağlantısını güncelleme komutu (admin). Bulunamazsa
    /// NotFound (404). Başarıda güncel <see cref="Common.SocialMediaResponse"/> döner.
    /// </summary>
    /// <param name="Id">Güncellenecek kaydın kimliği (route'tan gelir).</param>
    /// <param name="Title">Yeni platform adı.</param>
    /// <param name="Url">Yeni profil/hesap bağlantısı (geçerli mutlak URL).</param>
    /// <param name="Icon">Yeni ikon CSS sınıfı veya yolu.</param>
    public sealed record UpdateSocialMediaCommand(Guid Id, string Title, string Url, string Icon);
}
