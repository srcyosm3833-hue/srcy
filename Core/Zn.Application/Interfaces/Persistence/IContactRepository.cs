using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Features.Contact.Common;
using Zn.Domain.Entity;

namespace Zn.Application.Interfaces.Persistence
{
    /// <summary>
    /// Contact kalıcılaştırma sözleşmesi. İmplementasyon Zn.Persistence'tadır; Application
    /// katmanı EF Core'a doğrudan bağımlı olmadan tekil iletişim kaydını yönetir.
    /// <para>
    /// Uygulama yalnızca TEK bir Contact kaydı tutar (upsert ile yönetilir). Okuma sorgusu
    /// AsNoTracking + DB seviyesinde projeksiyon ile FirstOrDefault döner; upsert akışında yapılan
    /// okuma ise mevcut kaydı mutasyon için tracked döner.
    /// </para>
    /// </summary>
    public interface IContactRepository
    {
        /// <summary>
        /// Tekil iletişim kaydını dışa dönen projeksiyon olarak döner; hiç kayıt yoksa null.
        /// Yalnızca okuma amaçlıdır (AsNoTracking + projeksiyon).
        /// </summary>
        Task<ContactResponse?> GetAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Mevcut tekil iletişim kaydını takip edilen (tracked) entity olarak döner; hiç kayıt
        /// yoksa null. Upsert akışında "var mı?" kararı ve mutasyon için kullanılır.
        /// </summary>
        Task<Contact?> GetTrackedAsync(CancellationToken cancellationToken);

        /// <summary>Yeni bir iletişim kaydı ekler (henüz kaydedilmez; bkz. <see cref="SaveChangesAsync"/>).</summary>
        Task AddAsync(Contact contact, CancellationToken cancellationToken);

        /// <summary>Bekleyen değişiklikleri veritabanına yazar.</summary>
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
