using System;
using System.Collections.Generic;
using System.Linq;

namespace Zn.Domain.Authorization
{
    /// <summary>
    /// Uygulamadaki rol adlarının tek doğruluk kaynağı. Hem rol seed'inde hem de
    /// <c>[Authorize(Roles = ...)]</c> attribute'larında bu sabitler kullanılmalıdır;
    /// böylece serbest string'lerden kaynaklı uyuşmazlıklar (örn. yazım hatası) önlenir.
    /// </summary>
    public static class RoleNames
    {
        /// <summary>Yönetici rolü: tüm içerik ve kullanıcı/rol yönetimi yetkisine sahip en üst seviye.</summary>
        public const string Admin = "Admin";

        /// <summary>
        /// İçerik yöneticisi rolü: kategori/blog/mesaj gibi içerik yönetimini Admin'den bağımsız
        /// yürütebilir. Ancak kullanıcı güncelleme/silme ve rol atama yetkisi yoktur; başkasının
        /// blog veya yorumunu silemez (yalnızca kendi içeriğini yönetir). Bkz. A6 yetki matrisi.
        /// </summary>
        public const string Manager = "Manager";

        /// <summary>Standart kayıtlı kullanıcı rolü.</summary>
        public const string User = "User";

        /// <summary>Uygulama açılışında seed edilecek tüm roller.</summary>
        public static IReadOnlyList<string> All { get; } = new[] { Admin, Manager, User };

        /// <summary>
        /// Verilen rol adının sistem tarafından korunan bir rol (Admin/Manager/User) olup olmadığını döner.
        /// Korumalı roller yeniden adlandırılamaz ve silinemez; karşılaştırma büyük/küçük harf duyarsızdır.
        /// </summary>
        public static bool IsProtected(string roleName) =>
            !string.IsNullOrWhiteSpace(roleName)
            && All.Any(name => string.Equals(name, roleName, StringComparison.OrdinalIgnoreCase));
    }
}
