namespace Zn.Application.Common.Results
{
    /// <summary>
    /// Bir <see cref="Error"/>'ün hangi kategoriye girdiğini belirtir.
    /// Sunum katmanı (controller / global exception handler) bu değeri
    /// uygun HTTP durum koduna eşler:
    /// Validation → 400, NotFound → 404, Conflict → 409,
    /// Unauthorized → 401, Forbidden → 403, Locked → 423, Failure → 500.
    /// </summary>
    public enum ErrorType
    {
        /// <summary>Beklenmeyen / sınıflandırılmamış sunucu hatası (500).</summary>
        Failure = 0,

        /// <summary>İş kuralı veya girdi doğrulama hatası (400).</summary>
        Validation = 1,

        /// <summary>İstenen kayıt bulunamadı (404).</summary>
        NotFound = 2,

        /// <summary>Mevcut durumla çakışma; örn. benzersizlik ihlali (409).</summary>
        Conflict = 3,

        /// <summary>Kimlik doğrulanamadı veya kimlik bilgileri geçersiz (401).</summary>
        Unauthorized = 4,

        /// <summary>Kimlik doğrulandı ancak yetki yok (403).</summary>
        Forbidden = 5,

        /// <summary>Kaynak kilitli; örn. çok sayıda hatalı giriş sonrası hesap kilidi (423).</summary>
        Locked = 6
    }
}
