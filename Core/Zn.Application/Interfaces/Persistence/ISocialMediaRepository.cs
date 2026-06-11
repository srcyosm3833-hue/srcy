using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Features.SocialMedia.Common;
using DomainSocialMedia = Zn.Domain.Entity.SocialMedia;

namespace Zn.Application.Interfaces.Persistence
{
    /// <summary>
    /// SocialMedia kalıcılaştırma sözleşmesi. İmplementasyon Zn.Persistence'tadır;
    /// Application katmanı EF Core'a doğrudan bağımlı olmadan sosyal medya CRUD'unu yönetir.
    /// <para>
    /// Liste sorgusu yalnızca okuma amaçlıdır (AsNoTracking + DB seviyesinde projeksiyon);
    /// güncelleme/silme için yapılan tekil okuma tracked entity döner.
    /// </para>
    /// </summary>
    public interface ISocialMediaRepository
    {
        /// <summary>
        /// Tüm sosyal medya bağlantılarını başlığa göre sıralı döner. Yalnızca okuma amaçlıdır
        /// (AsNoTracking + DB seviyesinde projeksiyon). Kayıt yoksa boş liste döner.
        /// </summary>
        Task<IReadOnlyList<SocialMediaListItem>> GetAllAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Verilen Id'ye sahip kaydı takip edilen (tracked) entity olarak döner; yoksa null.
        /// Güncelleme/silme akışlarında, üzerinde değişiklik yapılıp kaydedilecekse kullanılır.
        /// </summary>
        Task<DomainSocialMedia?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Verilen başlığa sahip bir kayıt var mı (büyük/küçük harf duyarsız)? Duplicate kontrolü
        /// için kullanılır; <paramref name="excludeId"/> verilirse o Id'li kayıt hariç tutulur
        /// (güncellemede kaydın kendisiyle çakışmasını önlemek için).
        /// </summary>
        Task<bool> ExistsByTitleAsync(string title, Guid? excludeId, CancellationToken cancellationToken);

        /// <summary>Yeni bir kayıt ekler (henüz kaydedilmez; bkz. <see cref="SaveChangesAsync"/>).</summary>
        Task AddAsync(DomainSocialMedia socialMedia, CancellationToken cancellationToken);

        /// <summary>Takip edilen bir kaydı silmek üzere işaretler (bkz. <see cref="SaveChangesAsync"/>).</summary>
        void Remove(DomainSocialMedia socialMedia);

        /// <summary>Bekleyen değişiklikleri veritabanına yazar.</summary>
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
