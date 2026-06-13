namespace Zn.Application.Features.Users.CreateUser
{
    /// <summary>
    /// Admin tarafından yeni bir kullanıcı oluşturma komutu. Oluşturulan kullanıcıya varsayılan
    /// "User" rolü atanır. Yetki: yalnızca Admin. Başarıda oluşturulan kullanıcının temsili döner.
    /// </summary>
    /// <param name="FirstName">Kullanıcının adı.</param>
    /// <param name="LastName">Kullanıcının soyadı.</param>
    /// <param name="Email">Kullanıcının e-posta adresi (aynı zamanda kullanıcı adı olur).</param>
    /// <param name="Password">İlk şifre (Identity politikasına uymalıdır).</param>
    /// <param name="ImageUrl">
    /// Profil görseli URL'i. Boş/null gönderilirse handler varsayılan avatarı uygular.
    /// </param>
    public sealed record CreateUserByAdminCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string? ImageUrl);
}
