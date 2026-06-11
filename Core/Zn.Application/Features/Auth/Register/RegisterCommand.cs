namespace Zn.Application.Features.Auth.Register
{
    /// <summary>
    /// Yeni kullanıcı kaydı komutu. Immutable record; alan doğrulaması
    /// <see cref="RegisterCommandValidator"/> tarafından yapılır.
    /// Sonuç olarak oluşturulan kullanıcının Id ve e-postasını taşıyan
    /// <see cref="RegisterResponse"/> döner; şifre hiçbir yerde geri dönmez.
    /// </summary>
    /// <param name="FirstName">Kullanıcının adı.</param>
    /// <param name="LastName">Kullanıcının soyadı.</param>
    /// <param name="Email">Benzersiz e-posta adresi (aynı zamanda kullanıcı adı olur).</param>
    /// <param name="Password">Düz şifre; yalnızca hash'lenip saklanır.</param>
    /// <param name="ImageUrl">Profil görseli yolu/URL'i.</param>
    public sealed record RegisterCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string ImageUrl);
}
