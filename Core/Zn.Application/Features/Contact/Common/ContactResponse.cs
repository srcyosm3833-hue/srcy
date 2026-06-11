using System;

namespace Zn.Application.Features.Contact.Common
{
    /// <summary>
    /// Dışa dönen iletişim bilgisi yanıtı. Uygulama tek bir Contact kaydı tuttuğu için bu yanıt
    /// hem public GET hem admin upsert akışlarında kullanılır.
    /// </summary>
    /// <param name="Id">İletişim kaydının benzersiz kimliği.</param>
    /// <param name="Address">Açık adres.</param>
    /// <param name="Email">E-posta adresi.</param>
    /// <param name="Phone">Telefon numarası.</param>
    /// <param name="MapUrl">Harita (embed/konum) URL'i.</param>
    public sealed record ContactResponse(
        Guid Id,
        string Address,
        string Email,
        string Phone,
        string MapUrl);
}
