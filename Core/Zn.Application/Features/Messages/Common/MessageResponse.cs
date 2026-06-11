using System;

namespace Zn.Application.Features.Messages.Common
{
    /// <summary>
    /// Yönetici mesaj kutusuna dışa dönen mesaj yanıtı. Tüm iletişim formu alanları + okunma
    /// durumu + oluşturulma zamanını içerir.
    /// </summary>
    /// <param name="Id">Mesajın benzersiz kimliği.</param>
    /// <param name="Name">Gönderenin adı.</param>
    /// <param name="Email">Gönderenin e-posta adresi.</param>
    /// <param name="Subject">Mesaj konusu.</param>
    /// <param name="MessageBody">Mesaj içeriği.</param>
    /// <param name="IsRead">Mesaj okundu mu?</param>
    /// <param name="CreatedAt">Oluşturulma anı (UTC).</param>
    public sealed record MessageResponse(
        Guid Id,
        string Name,
        string Email,
        string Subject,
        string MessageBody,
        bool IsRead,
        DateTime CreatedAt);
}
