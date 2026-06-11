using Zn.Application.Common.Results;

namespace Zn.Application.Features.Auth.Common
{
    /// <summary>
    /// Auth dikey diliminde tekrar eden hata tanımlarını tek noktada toplar.
    /// Kullanıcı varlığını sızdırmamak için login/refresh tarafında bilinçli olarak
    /// jenerik mesajlar kullanılır.
    /// </summary>
    public static class AuthErrors
    {
        /// <summary>E-posta zaten kayıtlı (409).</summary>
        public static Error EmailAlreadyExists(string email) =>
            Error.Conflict("Auth.EmailAlreadyExists", $"A user with email '{email}' already exists.");

        /// <summary>
        /// Geçersiz kimlik bilgileri (401). Kullanıcı yok ya da şifre yanlış ayrımı
        /// yapılmaz; her iki durumda da aynı jenerik hata döner.
        /// </summary>
        public static readonly Error InvalidCredentials =
            Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");

        /// <summary>Hesap, çok sayıda hatalı giriş nedeniyle geçici olarak kilitli (423).</summary>
        public static readonly Error AccountLocked =
            Error.Locked("Auth.AccountLocked", "The account is temporarily locked due to multiple failed login attempts. Please try again later.");

        /// <summary>Refresh token geçersiz, süresi dolmuş veya revoke edilmiş (401).</summary>
        public static readonly Error InvalidRefreshToken =
            Error.Unauthorized("Auth.InvalidRefreshToken", "The refresh token is invalid, expired or revoked.");

        /// <summary>Identity'nin ürettiği kullanıcı oluşturma hataları (400).</summary>
        public static Error IdentityFailure(System.Collections.Generic.IReadOnlyDictionary<string, string[]> validations) =>
            Error.Validation("Auth.RegistrationFailed", "User registration failed.", validations);
    }
}
