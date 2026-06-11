using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Features.Comments.Common;
using Zn.Domain.Entity;

namespace Zn.Application.Interfaces.Persistence
{
    /// <summary>
    /// Comment kalıcılaştırma sözleşmesi. İmplementasyon Zn.Persistence'tadır; Application
    /// katmanı EF Core'a doğrudan bağımlı olmadan yorum CRUD'unu yönetir.
    /// <para>
    /// Okuma sorguları AsNoTracking + veritabanı seviyesinde projeksiyon kullanır (alt yorum
    /// sayısı COUNT ile hesaplanır, SubComments koleksiyonu belleğe çekilmez). Güncelleme/silme
    /// için yapılan okumalar yetki kontrolü ve mutasyon amacıyla tracked entity döner.
    /// </para>
    /// </summary>
    public interface ICommentRepository
    {
        /// <summary>
        /// Bir bloga ait yorumları createdAt azalan sıralı, sayfalanmış olarak ve toplam
        /// sayısıyla birlikte döner. Yalnızca okuma amaçlıdır (AsNoTracking + DB seviyesinde
        /// projeksiyon; alt yorum sayısı COUNT ile).
        /// </summary>
        /// <returns>Geçerli sayfadaki liste öğeleri ve bloga ait toplam yorum sayısı.</returns>
        Task<(IReadOnlyList<CommentListItem> Items, int TotalCount)> GetPagedByBlogIdAsync(
            Guid blogId,
            int page,
            int pageSize,
            CancellationToken cancellationToken);

        /// <summary>Verilen Id'ye sahip bir blog var mı? Yorum listeleme/ekleme öncesi doğrulama için.</summary>
        Task<bool> BlogExistsAsync(Guid blogId, CancellationToken cancellationToken);

        /// <summary>
        /// Verilen Id'ye sahip yorumu tek bir liste öğesi projeksiyonu olarak döner (alt yorum
        /// sayısı + yazar adı dahil); yoksa null. Create/Update sonrası yanıt üretmek için.
        /// Yalnızca okuma amaçlıdır (AsNoTracking + projeksiyon).
        /// </summary>
        Task<CommentListItem?> GetResponseByIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Verilen Id'ye sahip yorumu takip edilen (tracked) entity olarak döner; yoksa null.
        /// Güncelleme/silme akışlarında yetki kontrolü (UserId) ve mutasyon için kullanılır.
        /// </summary>
        Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>Yeni bir yorum ekler (henüz kaydedilmez; bkz. <see cref="SaveChangesAsync"/>).</summary>
        Task AddAsync(Comment comment, CancellationToken cancellationToken);

        /// <summary>
        /// Takip edilen bir yorumu silmek üzere işaretler (bkz. <see cref="SaveChangesAsync"/>).
        /// Comment → SubComment ilişkisi Cascade olduğundan alt yorumlar veritabanı tarafından
        /// otomatik silinir; ekstra işlem gerekmez.
        /// </summary>
        void Remove(Comment comment);

        /// <summary>Bekleyen değişiklikleri veritabanına yazar.</summary>
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
