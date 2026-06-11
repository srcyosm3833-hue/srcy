using System;

namespace Zn.Application.Features.SocialMedia.Delete
{
    /// <summary>
    /// Bir sosyal medya bağlantısını silme komutu (admin). Bulunamazsa NotFound (404).
    /// Başarıda değer taşımayan bir sonuç döner (HTTP 204).
    /// </summary>
    /// <param name="Id">Silinecek kaydın kimliği.</param>
    public sealed record DeleteSocialMediaCommand(Guid Id);
}
