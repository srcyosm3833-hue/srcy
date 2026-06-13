using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Features.Blogs.Common;
using Zn.Domain.Entity;

namespace Zn.Application.Interfaces.Persistence
{
    /// <summary>
    /// Blog kalıcılaştırma sözleşmesi. İmplementasyon Zn.Persistence'tadır; Application
    /// katmanı EF Core'a doğrudan bağımlı olmadan blog CRUD'unu yönetir.
    /// <para>
    /// Okuma sorguları AsNoTracking + veritabanı seviyesinde projeksiyon kullanır (liste
    /// sorgusunda Description gibi ağır alanlar çekilmez). Güncelleme/silme için yapılan
    /// okumalar yetki kontrolü ve mutasyon amacıyla tracked entity döner.
    /// </para>
    /// </summary>
    public interface IBlogRepository
    {
        /// <summary>
        /// Blogları createdAt azalan sıralı, sayfalanmış olarak ve toplam sayısıyla birlikte döner.
        /// <paramref name="categoryId"/> verilirse yalnızca o kategoriye ait bloglar döner.
        /// Yalnızca okuma amaçlıdır (AsNoTracking + DB seviyesinde projeksiyon).
        /// <para>
        /// <paramref name="includeDeleted"/> true ise soft delete edilmiş bloglar da dahil edilir
        /// (global query filter <c>IgnoreQueryFilters()</c> ile bypass edilir); yalnızca Admin/Manager
        /// sorgularında kullanılmalıdır. Varsayılan (false) public davranıştır.
        /// </para>
        /// </summary>
        /// <param name="currentUserId">
        /// Verilirse her blog için "bu kullanıcı beğendi mi" (IsLikedByCurrentUser) DB seviyesinde
        /// (EXISTS) hesaplanır; null ise daima false döner (anonim).
        /// </param>
        /// <returns>Geçerli sayfadaki liste öğeleri ve filtreye uyan toplam kayıt sayısı.</returns>
        Task<(IReadOnlyList<BlogListItem> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            Guid? categoryId,
            bool includeDeleted,
            string? currentUserId,
            CancellationToken cancellationToken);

        /// <summary>
        /// Blogları serbest metin (<paramref name="q"/>) üzerinde başlık VEYA açıklamada (LIKE)
        /// arar; sayfalanmış liste öğeleri ve filtreye uyan toplam sayıyla döner. CreatedAt azalan sıralı.
        /// <paramref name="categoryId"/> verilirse yalnızca o kategoriye ait bloglar içinde aranır.
        /// Yalnızca okuma amaçlıdır (AsNoTracking + DB seviyesinde projeksiyon).
        /// <para>
        /// Soft delete edilmiş bloglar global query filter (IsDeleted == false) ile otomatik dışlanır
        /// (bypass edilmez); arama yalnızca public, silinmemiş blogları döner.
        /// </para>
        /// </summary>
        /// <param name="currentUserId">
        /// Verilirse her blog için "bu kullanıcı beğendi mi" (IsLikedByCurrentUser) DB seviyesinde
        /// (EXISTS) hesaplanır; null ise daima false döner (anonim).
        /// </param>
        /// <returns>Geçerli sayfadaki liste öğeleri ve filtreye uyan toplam kayıt sayısı.</returns>
        Task<(IReadOnlyList<BlogListItem> Items, int TotalCount)> SearchAsync(
            string q,
            Guid? categoryId,
            int page,
            int pageSize,
            string? currentUserId,
            CancellationToken cancellationToken);

        /// <summary>
        /// Verilen Id'ye sahip blogu tam detayıyla (kategori + yazar bilgisi dahil) döner; yoksa null.
        /// Yalnızca okuma amaçlıdır (AsNoTracking + projeksiyon).
        /// <para>
        /// <paramref name="currentUserId"/> verilirse "bu kullanıcı beğendi mi" (IsLikedByCurrentUser)
        /// DB seviyesinde (EXISTS) hesaplanır; null ise false döner (anonim).
        /// </para>
        /// </summary>
        Task<BlogDetail?> GetDetailByIdAsync(Guid id, string? currentUserId, CancellationToken cancellationToken);

        /// <summary>
        /// Verilen Id'ye sahip blogu admin audit detayıyla (kategori + yazar + <c>CreatorIpHash</c>)
        /// döner; yoksa null. Yalnızca Admin sorgu yolunda kullanılır — audit alanı public yola
        /// sızmaz. Yalnızca okuma amaçlıdır (AsNoTracking + projeksiyon). Soft delete edilmiş
        /// bloglar da görülebilsin diye global query filter <c>IgnoreQueryFilters()</c> ile bypass
        /// edilir (admin denetimi için).
        /// </summary>
        Task<BlogAuditDetail?> GetAuditDetailByIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Verilen Id'ye sahip blogu takip edilen (tracked) entity olarak döner; yoksa null.
        /// Güncelleme/silme akışlarında yetki kontrolü (UserId) ve mutasyon için kullanılır.
        /// </summary>
        Task<Blog?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>Verilen Id'ye sahip bir kategori var mı? Create/Update'te kategori doğrulaması için.</summary>
        Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken);

        /// <summary>Yeni bir blog ekler (henüz kaydedilmez; bkz. <see cref="SaveChangesAsync"/>).</summary>
        Task AddAsync(Blog blog, CancellationToken cancellationToken);

        /// <summary>Bekleyen değişiklikleri veritabanına yazar.</summary>
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
