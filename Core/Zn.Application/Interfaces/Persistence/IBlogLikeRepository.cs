using System;
using System.Threading;
using System.Threading.Tasks;

namespace Zn.Application.Interfaces.Persistence
{
    /// <summary>
    /// BlogLike (blog beğenisi) kalıcılaştırma sözleşmesi. İmplementasyon Zn.Persistence'tadır;
    /// Application katmanı EF Core'a doğrudan bağımlı olmadan beğeni toggle akışını yönetir.
    /// <para>
    /// Toggle işleminin atomik/idempotent davranışı (eş zamanlı çift istekte duplicate üretmeme)
    /// implementasyonun sorumluluğundadır: composite PK (BlogId, UserId) ile DB seviyesinde garanti
    /// edilir, oluşabilecek eş zamanlılık hatası implementasyonda yakalanır. Böylece EF Core
    /// bağımlılığı (DbUpdateException) Application katmanına sızmaz.
    /// </para>
    /// </summary>
    public interface IBlogLikeRepository
    {
        /// <summary>Verilen Id'ye sahip (ve soft-delete edilmemiş) bir blog var mı? Toggle öncesi 404 kararı için.</summary>
        Task<bool> BlogExistsAsync(Guid blogId, CancellationToken cancellationToken);

        /// <summary>
        /// Belirtilen (blog, kullanıcı) çifti için beğeniyi açıp kapatır (toggle) ve sonucu döner.
        /// Mevcut beğeni varsa kaldırılır (Liked=false), yoksa eklenir (Liked=true). İşlem
        /// idempotenttir: eş zamanlı çift istek composite PK sayesinde duplicate üretmez. Dönen
        /// LikeCount, işlem sonrası bloğun güncel toplam beğeni sayısıdır.
        /// </summary>
        Task<(bool Liked, int LikeCount)> ToggleAsync(
            Guid blogId,
            string userId,
            CancellationToken cancellationToken);
    }
}
