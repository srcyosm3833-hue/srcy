namespace Zn.Application.Features.Contact.Upsert
{
    /// <summary>
    /// Tekil iletişim bilgisini ekleyen veya güncelleyen komut (upsert). Yalnızca Admin erişebilir.
    /// <para>
    /// Upsert mantığı: mevcut tek kayıt yoksa oluşturulur (201), varsa güncellenir (200). İKİNCİ BİR
    /// KAYIT ASLA OLUŞMAZ — uygulama tek bir Contact kaydı tutar. Başarıda güncel/oluşturulmuş
    /// kaydın <see cref="Common.ContactResponse"/>'u döner.
    /// </para>
    /// </summary>
    /// <param name="Address">Açık adres (zorunlu).</param>
    /// <param name="Email">E-posta adresi (zorunlu, geçerli format).</param>
    /// <param name="Phone">Telefon numarası (zorunlu).</param>
    /// <param name="MapUrl">Harita (embed/konum) URL'i (zorunlu).</param>
    public sealed record UpsertContactCommand(
        string Address,
        string Email,
        string Phone,
        string MapUrl);
}
