namespace Zn.Application.Features.Users.RemoveRole
{
    /// <summary>
    /// Mevcut bir kullanıcıdan rol kaldırma komutu. Yetki: yalnızca Admin (controller'da uygulanır).
    /// Kullanıcı yoksa 404; rol yoksa 404; sistemdeki son Admin'den Admin rolü kaldırılmak istenirse 400.
    /// Başarıda içerik dönmez (204).
    /// </summary>
    /// <param name="UserId">Rolü kaldırılacak kullanıcının kimliği (route'tan gelir).</param>
    /// <param name="RoleName">Kaldırılacak rolün adı (route'tan gelir).</param>
    public sealed record RemoveRoleCommand(string UserId, string RoleName);
}
