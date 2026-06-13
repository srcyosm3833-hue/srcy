namespace Zn.Application.Features.Users.AssignRole
{
    /// <summary>
    /// Mevcut bir kullanıcıya rol atama komutu. Yetki: yalnızca Admin (controller'da uygulanır).
    /// Kullanıcı yoksa 404; rol yoksa 404; kullanıcı zaten bu roldeyse idempotent başarı. Başarıda
    /// kullanıcının güncel temsili (rolleriyle) döner.
    /// </summary>
    /// <param name="UserId">Rol atanacak kullanıcının kimliği (route'tan gelir).</param>
    /// <param name="RoleName">Atanacak rolün adı (gövdeden gelir).</param>
    public sealed record AssignRoleCommand(string UserId, string RoleName);
}
