using System;

namespace Zn.Application.Interfaces.Authentication
{
    /// <summary>
    /// Üretilen bir access token'ı ve son kullanma anını taşıyan DTO.
    /// Refresh token string'i bilinçli olarak ayrı üretilir
    /// (bkz. <see cref="IJwtTokenService.GenerateRefreshToken"/>), çünkü
    /// refresh token'ın yaşam döngüsü (saklama, rotation, revoke) Faz 1'de
    /// RefreshToken entity'si üzerinden yönetilecektir.
    /// </summary>
    /// <param name="Value">İmzalanmış JWT access token string'i.</param>
    /// <param name="ExpiresAtUtc">Access token'ın UTC olarak son geçerlilik anı.</param>
    public sealed record AccessToken(string Value, DateTime ExpiresAtUtc);
}
