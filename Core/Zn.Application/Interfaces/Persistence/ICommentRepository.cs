using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Features.Comments.Common;
using Zn.Application.Features.Comments.GetAllForAdmin;
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
        /// <param name="currentUserId">
        /// Verilirse her yorum için "bu kullanıcı beğendi mi" (IsLikedByCurrentUser) DB seviyesinde
        /// (EXISTS) hesaplanır; null ise daima false döner (anonim).
        /// </param>
        /// <returns>Geçerli sayfadaki liste öğeleri ve bloga ait toplam yorum sayısı.</returns>
        Task<(IReadOnlyList<CommentListItem> Items, int TotalCount)> GetPagedByBlogIdAsync(
            Guid blogId,
            int page,
            int pageSize,
            string? currentUserId,
            CancellationToken cancellationToken);

        /// <summary>
        /// Tüm bloglardaki yorumları VE alt yorumları tek bir DÜZ (flat) moderasyon kümesinde,
        /// createdAt azalan sıralı + sayfalanmış olarak ve toplam sayısıyla birlikte döner (admin).
        /// Comments ve SubComments tabloları ayrı projeksiyonlarla ortak <see cref="CommentModerationItem"/>
        /// şekline çevrilip veritabanı seviyesinde birleştirilir (Concat); sayfalama ve sıralama DB
        /// tarafında uygulanır. Yalnızca okuma amaçlıdır (AsNoTracking + DB seviyesinde projeksiyon).
        /// <para>
        /// Comment ve SubComment global query filter'ları (silinmiş blogun yorumları + silinmiş
        /// kullanıcının alt yorumları otomatik dışlanır) burada bypass EDİLMEZ; moderasyon listesi
        /// yalnızca aktif kayıtları döner.
        /// </para>
        /// </summary>
        /// <returns>Geçerli sayfadaki düz moderasyon öğeleri ve filtreye uyan toplam (yorum + alt yorum) sayısı.</returns>
        Task<(IReadOnlyList<CommentModerationItem> Items, int TotalCount)> GetPagedForModerationAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken);

        /// <summary>Verilen Id'ye sahip bir blog var mı? Yorum listeleme/ekleme öncesi doğrulama için.</summary>
        Task<bool> BlogExistsAsync(Guid blogId, CancellationToken cancellationToken);

        /// <summary>
        /// Verilen Id'ye sahip yorumu tek bir liste öğesi projeksiyonu olarak döner (alt yorum
        /// sayısı + yazar adı + beğeni bilgisi dahil); yoksa null. Create/Update sonrası yanıt üretmek için.
        /// Yalnızca okuma amaçlıdır (AsNoTracking + projeksiyon).
        /// <para>
        /// <paramref name="currentUserId"/> verilirse "bu kullanıcı beğendi mi" (IsLikedByCurrentUser)
        /// DB seviyesinde (EXISTS) hesaplanır; null ise false döner.
        /// </para>
        /// </summary>
        Task<CommentListItem?> GetResponseByIdAsync(Guid id, string? currentUserId, CancellationToken cancellationToken);

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
