namespace Zn.Application.Features.Roles.UpdateRole
{
    /// <summary>
    /// Mevcut bir rolü yeniden adlandırma komutu. Yetki: yalnızca Admin (controller'da uygulanır).
    /// Korumalı roller (Admin/Manager/User) güncellenemez (400); rol bulunamazsa 404; yeni ad başka
    /// bir rolde kullanılıyorsa 409. Route'taki id otoritatiftir.
    /// </summary>
    /// <param name="RoleId">Güncellenecek rolün kimliği (route'tan gelir).</param>
    /// <param name="Name">Rolün yeni adı.</param>
    public sealed record UpdateRoleCommand(string RoleId, string Name);
}
