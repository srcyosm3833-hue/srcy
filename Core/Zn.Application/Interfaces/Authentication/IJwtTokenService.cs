namespace Zn.Application.Interfaces.Authentication
{
    /// <summary>
    /// JWT access token üretimi ve ham refresh token string üretimi sözleşmesi.
    /// İmplementasyon Zn.Infrastructure'da yer alır.
    /// <para>
    /// Tasarım notu: Bu arayüz RefreshToken entity'sine bağımlı değildir. Refresh token
    /// burada yalnızca kriptografik olarak güçlü, opak bir string olarak üretilir; onu
    /// kalıcılaştırma, hash'leme, rotation ve revoke işlemleri Faz 1'de RefreshToken
    /// entity'si ve ilgili handler'larda gerçekleştirilecektir.
    /// </para>
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Verilen kullanıcı bilgisi ve rollerden imzalanmış bir JWT access token üretir.
        /// </summary>
        /// <param name="user">Token'a gömülecek kullanıcı kimliği, e-posta ve roller.</param>
        /// <returns>Token string'i ve UTC son kullanma anını içeren <see cref="AccessToken"/>.</returns>
        AccessToken GenerateAccessToken(TokenUser user);

        /// <summary>
        /// Kriptografik olarak güçlü, opak bir refresh token string'i üretir.
        /// Saklama/rotation/revoke çağıranın (Faz 1 handler'larının) sorumluluğundadır.
        /// </summary>
        /// <returns>Base64-url benzeri, tahmin edilemez bir token string'i.</returns>
        string GenerateRefreshToken();
    }
}
