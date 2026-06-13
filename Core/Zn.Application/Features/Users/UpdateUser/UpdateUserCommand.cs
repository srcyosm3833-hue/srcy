namespace Zn.Application.Features.Users.UpdateUser
{
    /// <summary>
    /// Admin tarafından bir kullanıcının profil bilgilerini (ad, soyad, görsel) güncelleme komutu.
    /// E-posta ve rol bu kapsamda değildir (rol yönetimi ayrı dilimdir; A6 matrisi). Yetki: yalnızca Admin.
    /// </summary>
    /// <param name="UserId">Güncellenecek kullanıcının kimliği (route'tan; otoritatif).</param>
    /// <param name="FirstName">Kullanıcının yeni adı.</param>
    /// <param name="LastName">Kullanıcının yeni soyadı.</param>
    /// <param name="ImageUrl">
    /// Profil görseli URL'i. Boş/null gönderilirse handler varsayılan avatarı uygular
    /// (DB sütunu NOT NULL olduğu için boş bırakılamaz).
    /// </param>
    public sealed record UpdateUserCommand(
        string UserId,
        string FirstName,
        string LastName,
        string? ImageUrl);
}
