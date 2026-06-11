namespace Zn.Infrastructure.Authentication
{
    /// <summary>
    /// JWT üretimi ve doğrulaması için yapılandırma değerleri.
    /// appsettings'teki "JwtSettings" bölümünden bağlanır (bind).
    /// SecretKey production'da user secrets / environment / key vault'tan gelmelidir;
    /// appsettings.json'a gerçek secret yazılmaz.
    /// </summary>
    public sealed class JwtSettings
    {
        /// <summary>Yapılandırma bölümünün adı; binding'de kullanılır.</summary>
        public const string SectionName = "JwtSettings";

        /// <summary>Token'ı üreten taraf (iss claim'i).</summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>Token'ın hedef kitlesi (aud claim'i).</summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Token imzalama anahtarı (HMAC-SHA256 için simetrik). En az 32 bayt olmalıdır.
        /// Production'da gizli tutulur; appsettings.json'a placeholder dışında yazılmaz.
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>Access token'ın geçerlilik süresi (dakika). Önerilen: ~15.</summary>
        public int AccessTokenMinutes { get; set; } = 15;

        /// <summary>Refresh token'ın geçerlilik süresi (gün). Önerilen: ~7.</summary>
        public int RefreshTokenDays { get; set; } = 7;
    }
}
