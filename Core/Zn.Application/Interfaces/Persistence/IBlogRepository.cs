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
        /// </summary>
        /// <returns>Geçerli sayfadaki liste öğeleri ve filtreye uyan toplam kayıt sayısı.</returns>
        Task<(IReadOnlyList<BlogListItem> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            Guid? categoryId,
            CancellationToken cancellationToken);

        /// <summary>
        /// Verilen Id'ye sahip blogu tam detayıyla (kategori + yazar bilgisi dahil) döner; yoksa null.
        /// Yalnızca okuma amaçlıdır (AsNoTracking + projeksiyon).
        /// </summary>
        Task<BlogDetail?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Verilen Id'ye sahip blogu takip edilen (tracked) entity olarak döner; yoksa null.
        /// Güncelleme/silme akışlarında yetki kontrolü (UserId) ve mutasyon için kullanılır.
        /// </summary>
        Task<Blog?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>Verilen Id'ye sahip bir kategori var mı? Create/Update'te kategori doğrulaması için.</summary>
        Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken);

        /// <summary>Yeni bir blog ekler (henüz kaydedilmez; bkz. <see cref="SaveChangesAsync"/>).</summary>
        Task AddAsync(Blog blog, CancellationToken cancellationToken);

        /// <summary>Takip edilen bir blogu silmek üzere işaretler (bkz. <see cref="SaveChangesAsync"/>).</summary>
        void Remove(Blog blog);

        /// <summary>Bekleyen değişiklikleri veritabanına yazar.</summary>
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
