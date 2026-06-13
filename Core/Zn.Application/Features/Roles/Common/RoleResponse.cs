namespace Zn.Application.Features.Roles.Common
{
    /// <summary>
    /// Rol yönetimi uçlarının dışa döndürdüğü rol temsili. Listeleme, oluşturma ve güncelleme
    /// sonrası yanıtlarda kullanılır.
    /// </summary>
    /// <param name="Id">Rolün benzersiz kimliği (Identity string PK).</param>
    /// <param name="Name">Rol adı (örn. "Admin").</param>
    /// <param name="UserCount">Bu role atanmış kullanıcı sayısı. Listeleme dışında 0 olabilir.</param>
    /// <param name="IsProtected">
    /// Rol sistem tarafından korunuyorsa (Admin/Manager/User) true; bu roller güncellenemez/silinemez.
    /// Frontend bu alana göre Düzenle/Sil butonlarını devre dışı bırakır.
    /// </param>
    public sealed record RoleResponse(
        string Id,
        string Name,
        int UserCount,
        bool IsProtected);
}
