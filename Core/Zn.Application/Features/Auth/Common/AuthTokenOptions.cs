namespace Zn.Application.Features.Auth.Common
{
    /// <summary>
    /// Refresh token yaşam süresi gibi, Application katmanının ihtiyaç duyduğu
    /// auth ayarları. Infrastructure'daki JwtSettings'e bağımlı kalmamak için
    /// ayrı tutulur; aynı "JwtSettings" yapılandırma bölümünden bind edilir.
    /// </summary>
    public sealed class AuthTokenOptions
    {
        /// <summary>Bind edilecek yapılandırma bölümünün adı (JwtSettings ile aynı bölüm).</summary>
        public const string SectionName = "JwtSettings";

        /// <summary>Refresh token'ın geçerlilik süresi (gün). Önerilen: ~7.</summary>
        public int RefreshTokenDays { get; set; } = 7;
    }
}
