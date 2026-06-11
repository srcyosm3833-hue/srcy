using System.Collections.Generic;

namespace Zn.Domain.Authorization
{
    /// <summary>
    /// Uygulamadaki rol adlarının tek doğruluk kaynağı. Hem rol seed'inde hem de
    /// <c>[Authorize(Roles = ...)]</c> attribute'larında bu sabitler kullanılmalıdır;
    /// böylece serbest string'lerden kaynaklı uyuşmazlıklar (örn. yazım hatası) önlenir.
    /// </summary>
    public static class RoleNames
    {
        /// <summary>Yönetici rolü: kategori/blog yönetimi gibi ayrıcalıklı işlemler.</summary>
        public const string Admin = "Admin";

        /// <summary>Standart kayıtlı kullanıcı rolü.</summary>
        public const string User = "User";

        /// <summary>Uygulama açılışında seed edilecek tüm roller.</summary>
        public static IReadOnlyList<string> All { get; } = new[] { Admin, User };
    }
}
