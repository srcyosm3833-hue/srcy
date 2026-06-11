namespace Zn.Application.Features.Auth.Register
{
    /// <summary>
    /// Başarılı kayıt yanıtı. Yalnızca güvenli alanları içerir;
    /// şifre veya hash asla dönmez.
    /// </summary>
    /// <param name="Id">Oluşturulan kullanıcının Id'si (AspNetUsers.Id).</param>
    /// <param name="Email">Kayıtlı e-posta adresi.</param>
    public sealed record RegisterResponse(string Id, string Email);
}
