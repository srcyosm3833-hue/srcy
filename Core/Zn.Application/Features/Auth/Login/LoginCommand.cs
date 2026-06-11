namespace Zn.Application.Features.Auth.Login
{
    /// <summary>
    /// Kullanıcı giriş komutu. Başarıda access + refresh token çifti
    /// (<see cref="Common.AuthTokensResponse"/>) döner.
    /// </summary>
    /// <param name="Email">Kullanıcının e-posta adresi.</param>
    /// <param name="Password">Düz şifre; doğrulamada Identity'ye verilir, saklanmaz.</param>
    public sealed record LoginCommand(string Email, string Password);
}
