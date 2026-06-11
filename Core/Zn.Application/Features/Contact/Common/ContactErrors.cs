using Zn.Application.Common.Results;

namespace Zn.Application.Features.Contact.Common
{
    /// <summary>
    /// Contact dikey diliminde tekrar eden hata tanımlarını tek noktada toplar.
    /// Handler'lar bu fabrikalar üzerinden anlamlı <see cref="Error"/> üretir;
    /// ApiControllerBase bunları uygun HTTP koduna eşler.
    /// </summary>
    public static class ContactErrors
    {
        /// <summary>
        /// Henüz hiç iletişim kaydı oluşturulmamış (404). İlk kurulum öncesinde, yönetici
        /// upsert ile ilk kaydı oluşturana kadar public GET bu hatayı döner.
        /// </summary>
        public static Error NotFound() =>
            Error.NotFound("Contact.NotFound", "Contact information has not been configured yet.");
    }
}
