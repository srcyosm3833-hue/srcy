using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Features.Categories.Common;
using Zn.Domain.Entity;

namespace Zn.Application.Interfaces.Persistence
{
    /// <summary>
    /// Category kalıcılaştırma sözleşmesi. İmplementasyon Zn.Persistence'tadır;
    /// Application katmanı EF Core'a doğrudan bağımlı olmadan kategori CRUD'unu yönetir.
    /// <para>
    /// Liste sorgularında blog sayısı veritabanı seviyesinde projekte edilir
    /// (<see cref="GetAllWithBlogCountAsync"/>); tekil okumalar <see cref="AsNoTracking"/>,
    /// güncelleme/silme için yapılan okumalar tracked döner.
    /// </para>
    /// </summary>
    public interface ICategoryRepository
    {
        /// <summary>
        /// Tüm kategorileri, her birine ait blog sayısıyla birlikte ada göre sıralı döner.
        /// Yalnızca okuma amaçlıdır (AsNoTracking + DB seviyesinde projeksiyon).
        /// <para>
        /// <paramref name="includeDeleted"/> true ise soft delete edilmiş kategoriler de
        /// listeye dahil edilir (global query filter <c>IgnoreQueryFilters()</c> ile bypass edilir);
        /// yalnızca Admin/Manager sorgularında kullanılmalıdır. Varsayılan (false) public davranıştır.
        /// </para>
        /// </summary>
        Task<IReadOnlyList<CategoryWithBlogCount>> GetAllWithBlogCountAsync(
            bool includeDeleted,
            CancellationToken cancellationToken);

        /// <summary>
        /// Verilen Id'ye sahip kategoriyi blog sayısıyla birlikte döner; yoksa null.
        /// Yalnızca okuma amaçlıdır (AsNoTracking + projeksiyon).
        /// </summary>
        Task<CategoryWithBlogCount?> GetByIdWithBlogCountAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Verilen Id'ye sahip kategoriyi takip edilen (tracked) entity olarak döner; yoksa null.
        /// Güncelleme/silme akışlarında, üzerinde değişiklik yapılıp kaydedilecekse kullanılır.
        /// </summary>
        Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Verilen ada sahip bir kategori var mı (büyük/küçük harf duyarsız)?
        /// Duplicate kontrolü için kullanılır. <paramref name="excludeId"/> verilirse
        /// o Id'li kayıt hariç tutulur (güncellemede kendisiyle çakışmayı önlemek için).
        /// </summary>
        Task<bool> ExistsByNameAsync(string categoryName, Guid? excludeId, CancellationToken cancellationToken);

        /// <summary>Verilen kategorinin en az bir bloğa sahip olup olmadığını döner.</summary>
        Task<bool> HasBlogsAsync(Guid categoryId, CancellationToken cancellationToken);

        /// <summary>Yeni bir kategori ekler (henüz kaydedilmez; bkz. <see cref="SaveChangesAsync"/>).</summary>
        Task AddAsync(Category category, CancellationToken cancellationToken);

        /// <summary>Bekleyen değişiklikleri veritabanına yazar.</summary>
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
