using System.Collections.Generic;
using Zn.Application.Common.Results;

namespace Zn.Application.Features.Roles.Common
{
    /// <summary>
    /// Rol yönetimi (CRUD + atama) dikey dilimlerinde tekrar eden hata tanımlarını tek noktada toplar.
    /// Hata kodları "Role." önekiyle makinece okunabilir; mesajlar İngilizce'dir.
    /// </summary>
    public static class RoleErrors
    {
        /// <summary>Verilen Id veya ada sahip rol bulunamadı (404).</summary>
        public static Error NotFound(string roleIdOrName) =>
            Error.NotFound("Role.NotFound", $"No role was found with identifier '{roleIdOrName}'.");

        /// <summary>Aynı adda bir rol zaten mevcut (409).</summary>
        public static Error Conflict(string roleName) =>
            Error.Conflict("Role.Conflict", $"A role with name '{roleName}' already exists.");

        /// <summary>
        /// Sistem tarafından korunan rol (Admin/Manager/User) güncellenemez veya silinemez (400).
        /// </summary>
        public static Error ProtectedRole(string roleName) =>
            Error.Validation(
                "Role.Protected",
                $"The role '{roleName}' is protected by the system and cannot be modified or deleted.");

        /// <summary>Rolde aktif kullanıcı bulunduğundan rol silinemez (409).</summary>
        public static Error RoleHasUsers(string roleName) =>
            Error.Conflict(
                "Role.HasUsers",
                $"The role '{roleName}' cannot be deleted because there are active users assigned to it.");

        /// <summary>Identity'nin ürettiği rol oluşturma/güncelleme/silme hataları (400).</summary>
        public static Error IdentityFailure(IReadOnlyDictionary<string, string[]> validations) =>
            Error.Validation("Role.OperationFailed", "The role operation failed.", validations);
    }
}
