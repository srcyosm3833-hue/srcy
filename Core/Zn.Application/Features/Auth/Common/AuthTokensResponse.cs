using System;

namespace Zn.Application.Features.Auth.Common
{
    /// <summary>
    /// Login ve refresh işlemlerinin istemciye dönen ortak yanıtı.
    /// Access token kısa ömürlü (~15 dk), refresh token uzun ömürlüdür (~7 gün).
    /// Refresh token burada DÜZ haliyle döner; veritabanında yalnızca hash'i saklanır.
    /// Şifre/hash gibi gizli alanlar asla bu DTO'ya konmaz.
    /// </summary>
    /// <param name="AccessToken">İmzalanmış JWT access token.</param>
    /// <param name="AccessTokenExpiresAtUtc">Access token'ın UTC son geçerlilik anı.</param>
    /// <param name="RefreshToken">Düz refresh token string'i (yalnızca bu yanıtta görünür).</param>
    /// <param name="RefreshTokenExpiresAtUtc">Refresh token'ın UTC son geçerlilik anı.</param>
    public sealed record AuthTokensResponse(
        string AccessToken,
        DateTime AccessTokenExpiresAtUtc,
        string RefreshToken,
        DateTime RefreshTokenExpiresAtUtc);
}
