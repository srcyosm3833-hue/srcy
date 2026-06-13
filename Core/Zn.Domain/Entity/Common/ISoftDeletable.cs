using System;

namespace Zn.Domain.Entity.Common
{
    /// <summary>
    /// Soft delete (yumuşak silme) destekleyen entity'ler için sözleşme. Bu arayüzü uygulayan
    /// entity'ler kalıcı olarak silinmek yerine <see cref="IsDeleted"/> bayrağıyla "silinmiş"
    /// işaretlenir; böylece veri bütünlüğü korunur ve kayıt gerektiğinde kurtarılabilir.
    /// <para>
    /// EF Core global query filter'ı bu arayüzü uygulayan tüm entity tipleri için otomatik
    /// kurulur (<c>e => !e.IsDeleted</c>); bu sayede silinmiş kayıtlar varsayılan sorgularda
    /// (public API dahil) hiçbir zaman dönmez. Admin/Manager'ın silinmişleri görebilmesi için
    /// repository seviyesinde <c>IgnoreQueryFilters()</c> ile ayrı bir yol bırakılır.
    /// </para>
    /// </summary>
    public interface ISoftDeletable
    {
        /// <summary>Kayıt soft delete edilmişse true, aktifse false.</summary>
        bool IsDeleted { get; }

        /// <summary>Soft delete işleminin gerçekleştiği an (UTC); kayıt aktifse null.</summary>
        DateTime? DeletedAt { get; }
    }
}
