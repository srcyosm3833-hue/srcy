namespace Zn.Application.Features.Auth.Refresh
{
    /// <summary>
    /// Access token süresi dolduğunda yeni bir access + refresh token çifti almak için
    /// kullanılan komut. Düz refresh token istemciden gelir; başarıda rotation uygulanır
    /// (eski token revoke edilir, yeni çift üretilir).
    /// </summary>
    /// <param name="RefreshToken">İstemcinin elindeki düz refresh token string'i.</param>
    public sealed record RefreshTokenCommand(string RefreshToken);
}
