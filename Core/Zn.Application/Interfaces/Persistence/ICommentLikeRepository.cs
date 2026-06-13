using System;
using System.Threading;
using System.Threading.Tasks;

namespace Zn.Application.Interfaces.Persistence
{
    /// <summary>
    /// CommentLike (yorum beğenisi) kalıcılaştırma sözleşmesi. İmplementasyon Zn.Persistence'tadır;
    /// Application katmanı EF Core'a doğrudan bağımlı olmadan beğeni toggle akışını yönetir.
    /// <para>
    /// Toggle işleminin atomik/idempotent davranışı (eş zamanlı çift istekte duplicate üretmeme)
    /// implementasyonun sorumluluğundadır: composite PK (CommentId, UserId) ile DB seviyesinde garanti
    /// edilir, oluşabilecek eş zamanlılık hatası implementasyonda yakalanır. Böylece EF Core
    /// bağımlılığı (DbUpdateException) Application katmanına sızmaz.
    /// </para>
    /// </summary>
    public interface ICommentLikeRepository
    {
        /// <summary>Verilen Id'ye sahip bir yorum var mı? Toggle öncesi 404 kararı için.</summary>
        Task<bool> CommentExistsAsync(Guid commentId, CancellationToken cancellationToken);

        /// <summary>
        /// Belirtilen (yorum, kullanıcı) çifti için beğeniyi açıp kapatır (toggle) ve sonucu döner.
        /// Mevcut beğeni varsa kaldırılır (Liked=false), yoksa eklenir (Liked=true). İşlem
        /// idempotenttir: eş zamanlı çift istek composite PK sayesinde duplicate üretmez. Dönen
        /// LikeCount, işlem sonrası yorumun güncel toplam beğeni sayısıdır.
        /// </summary>
        Task<(bool Liked, int LikeCount)> ToggleAsync(
            Guid commentId,
            string userId,
            CancellationToken cancellationToken);
    }
}
