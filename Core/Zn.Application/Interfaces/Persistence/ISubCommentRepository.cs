using System;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Features.SubComments.Common;
using Zn.Domain.Entity;

namespace Zn.Application.Interfaces.Persistence
{
    /// <summary>
    /// SubComment kalıcılaştırma sözleşmesi. İmplementasyon Zn.Persistence'tadır; Application
    /// katmanı EF Core'a doğrudan bağımlı olmadan alt yorum CRUD'unu yönetir.
    /// <para>
    /// Okuma sorguları AsNoTracking + veritabanı seviyesinde projeksiyon kullanır. Güncelleme/silme
    /// için yapılan okumalar yetki kontrolü ve mutasyon amacıyla tracked entity döner.
    /// </para>
    /// </summary>
    public interface ISubCommentRepository
    {
        /// <summary>Verilen Id'ye sahip bir ana yorum var mı? Alt yorum ekleme öncesi doğrulama için.</summary>
        Task<bool> CommentExistsAsync(Guid commentId, CancellationToken cancellationToken);

        /// <summary>
        /// Verilen Id'ye sahip alt yorumu bir liste öğesi projeksiyonu olarak döner (yazar adı
        /// dahil); yoksa null. Create/Update sonrası yanıt üretmek için. Yalnızca okuma amaçlıdır
        /// (AsNoTracking + projeksiyon).
        /// </summary>
        Task<SubCommentListItem?> GetResponseByIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Verilen Id'ye sahip alt yorumu takip edilen (tracked) entity olarak döner; yoksa null.
        /// Güncelleme/silme akışlarında yetki kontrolü (UserId) ve mutasyon için kullanılır.
        /// </summary>
        Task<SubComment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>Yeni bir alt yorum ekler (henüz kaydedilmez; bkz. <see cref="SaveChangesAsync"/>).</summary>
        Task AddAsync(SubComment subComment, CancellationToken cancellationToken);

        /// <summary>Takip edilen bir alt yorumu silmek üzere işaretler (bkz. <see cref="SaveChangesAsync"/>).</summary>
        void Remove(SubComment subComment);

        /// <summary>Bekleyen değişiklikleri veritabanına yazar.</summary>
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
