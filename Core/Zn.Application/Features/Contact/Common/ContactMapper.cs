using Riok.Mapperly.Abstractions;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Contact.Common
{
    /// <summary>
    /// Riok.Mapperly kaynak-üretimli mapper'ı. Domain <see cref="Zn.Domain.Entity.Contact"/>
    /// entity'sini dışa dönen <see cref="ContactResponse"/>'a eşler. Upsert (Create/Update) sonrası
    /// elde kalan entity'den yanıt üretmek için kullanılır; tüm alanlar isim eşleşmesiyle birebir
    /// kopyalanır, hesaplanan alan yoktur. (Public GET yolunda repository doğrudan DB seviyesinde
    /// ContactResponse'a projekte eder.)
    /// </summary>
    [Mapper]
    public static partial class ContactMapper
    {
        /// <summary>Tracked/oluşturulmuş Contact entity'sini iletişim yanıtına eşler.</summary>
        public static partial ContactResponse ToResponse(Zn.Domain.Entity.Contact source);
    }
}
