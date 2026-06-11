namespace Zn.Application.Features.Auth.Logout
{
    /// <summary>
    /// Verilen refresh token'ı revoke ederek oturumu sonlandırma komutu.
    /// İdempotenttir: token bulunamasa veya zaten revoke edilmiş olsa bile
    /// başarı döner (istemci açısından sonuç aynıdır).
    /// </summary>
    /// <param name="RefreshToken">Sonlandırılacak oturumun düz refresh token'ı.</param>
    public sealed record LogoutCommand(string RefreshToken);
}
