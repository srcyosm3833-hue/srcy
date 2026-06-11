using Zn.Application.Features.Contact.Common;

namespace Zn.Application.Features.Contact.Upsert
{
    /// <summary>
    /// Upsert handler'ının dönüş taşıyıcısı. Controller'ın doğru HTTP kodunu (201 oluşturuldu vs.
    /// 200 güncellendi) seçebilmesi için, dönen iletişim yanıtının yanında kaydın yeni mi
    /// oluşturulduğunu (<see cref="WasCreated"/>) bildirir.
    /// </summary>
    /// <param name="Contact">Oluşturulan/güncellenen iletişim kaydının yanıtı.</param>
    /// <param name="WasCreated">true ise kayıt yeni oluşturuldu (201); false ise güncellendi (200).</param>
    public sealed record UpsertContactResult(ContactResponse Contact, bool WasCreated);
}
