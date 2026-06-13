namespace Zn.Application.Features.Roles.DeleteRole
{
    /// <summary>
    /// Bir rolü silme komutu. Yetki: yalnızca Admin (controller'da uygulanır). Korumalı roller
    /// (Admin/Manager/User) silinemez (400); rol bulunamazsa 404; role atanmış aktif kullanıcı varsa
    /// 409. Başarıda içerik dönmez (204).
    /// </summary>
    /// <param name="RoleId">Silinecek rolün kimliği (route'tan gelir).</param>
    public sealed record DeleteRoleCommand(string RoleId);
}
