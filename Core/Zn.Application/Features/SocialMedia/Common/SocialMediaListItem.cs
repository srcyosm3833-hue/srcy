using System;

namespace Zn.Application.Features.SocialMedia.Common
{
    /// <summary>
    /// Repository'nin liste sorgusunda veritabanı seviyesinde projekte ettiği ara DTO.
    /// SocialMedia entity'sinin yalnızca yanıt için gereken alanlarını taşır; böylece
    /// entity'yi belleğe çekmeden (AsNoTracking + DB projeksiyonu) okuma yapılır.
    /// Mapperly bu tipi <see cref="SocialMediaResponse"/>'a eşler.
    /// </summary>
    /// <param name="Id">Bağlantının benzersiz kimliği.</param>
    /// <param name="Title">Platform adı.</param>
    /// <param name="Url">Profil/hesap bağlantısı.</param>
    /// <param name="Icon">İkon CSS sınıfı veya ikon dosya yolu.</param>
    public sealed record SocialMediaListItem(
        Guid Id,
        string Title,
        string Url,
        string Icon);
}
