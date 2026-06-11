namespace Zn.Application.Features.Messages.Send
{
    /// <summary>
    /// İletişim formundan mesaj gönderme komutu. Herkese açıktır (anonim ziyaretçi).
    /// <para>
    /// Mesaj okunmamış (IsRead=false) olarak kaydedilir. Yanıt minimaldir: ziyaretçiye yalnızca
    /// onay döner, oluşturulan kaydın Id'si bilinçli olarak paylaşılmaz (gereksiz bilgi sızdırmamak
    /// ve enumerasyonu kolaylaştırmamak için).
    /// </para>
    /// </summary>
    /// <param name="Name">Gönderenin adı (zorunlu).</param>
    /// <param name="Email">Gönderenin e-posta adresi (zorunlu, geçerli format).</param>
    /// <param name="Subject">Mesaj konusu (zorunlu).</param>
    /// <param name="MessageBody">Mesaj içeriği (zorunlu).</param>
    public sealed record SendMessageCommand(
        string Name,
        string Email,
        string Subject,
        string MessageBody);
}
