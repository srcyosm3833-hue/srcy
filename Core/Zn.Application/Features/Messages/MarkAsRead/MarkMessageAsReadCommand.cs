using System;

namespace Zn.Application.Features.Messages.MarkAsRead
{
    /// <summary>
    /// Bir mesajın okunma durumunu açıkça (true/false) set eden komut. Yalnızca Admin erişebilir.
    /// <para>
    /// Explicit set: istek gövdesi yalnızca <see cref="IsRead"/> taşır; böylece yönetici mesajı hem
    /// "okundu" hem "okunmadı" olarak işaretleyebilir. Mesaj yoksa NotFound (404). Başarıda
    /// güncellenmiş mesajın <see cref="Common.MessageResponse"/>'u döner (200).
    /// </para>
    /// </summary>
    /// <param name="Id">Okunma durumu değiştirilecek mesajın kimliği (route'tan gelir).</param>
    /// <param name="IsRead">Yeni okunma durumu (true=okundu, false=okunmadı).</param>
    public sealed record MarkMessageAsReadCommand(Guid Id, bool IsRead);
}
