using System;

namespace Zn.Application.Features.Messages.Common
{
    /// <summary>
    /// Repository'nin liste sorgusunda veritabanı seviyesinde projekte ettiği ara DTO. Yönetici
    /// mesaj kutusunda gösterilen tüm alanları taşır. Mapperly bunu birebir
    /// <see cref="MessageResponse"/>'a eşler (hesaplanan alan yoktur).
    /// </summary>
    /// <param name="Id">Mesajın benzersiz kimliği.</param>
    /// <param name="Name">Gönderenin adı.</param>
    /// <param name="Email">Gönderenin e-posta adresi.</param>
    /// <param name="Subject">Mesaj konusu.</param>
    /// <param name="MessageBody">Mesaj içeriği.</param>
    /// <param name="IsRead">Mesaj okundu mu?</param>
    /// <param name="CreatedAt">Oluşturulma anı (UTC).</param>
    /// <param name="SenderIpHash">Gönderenin tuzlu SHA-256 IP hash'i (anonim audit); çözülemediyse null.</param>
    public sealed record MessageListItem(
        Guid Id,
        string Name,
        string Email,
        string Subject,
        string MessageBody,
        bool IsRead,
        DateTime CreatedAt,
        string? SenderIpHash);
}
