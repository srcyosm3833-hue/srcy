using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Features.Messages.Common;
using Zn.Domain.Entity;

namespace Zn.Application.Interfaces.Persistence
{
    /// <summary>
    /// Message kalıcılaştırma sözleşmesi. İmplementasyon Zn.Persistence'tadır; Application
    /// katmanı EF Core'a doğrudan bağımlı olmadan iletişim mesajlarını yönetir.
    /// <para>
    /// Listeleme sorgusu AsNoTracking + veritabanı seviyesinde projeksiyon kullanır; sıralama ve
    /// sayfalama DB tarafında uygulanır (okunmamışlar önce, her grup içinde CreatedAt azalan).
    /// Okunma durumu güncellemesi için yapılan okuma tracked entity döner.
    /// </para>
    /// </summary>
    public interface IMessageRepository
    {
        /// <summary>
        /// Mesajları sayfalanmış olarak ve toplam sayısıyla birlikte döner. Sıralama DB tarafında:
        /// önce okunmamışlar (IsRead=false), ardından her grup içinde CreatedAt azalan. Yalnızca
        /// okuma amaçlıdır (AsNoTracking + DB seviyesinde projeksiyon).
        /// </summary>
        /// <returns>Geçerli sayfadaki liste öğeleri ve toplam mesaj sayısı.</returns>
        Task<(IReadOnlyList<MessageListItem> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken);

        /// <summary>
        /// Verilen Id'ye sahip mesajı takip edilen (tracked) entity olarak döner; yoksa null.
        /// Okunma durumu güncelleme akışında mutasyon için kullanılır.
        /// </summary>
        Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>Yeni bir mesaj ekler (henüz kaydedilmez; bkz. <see cref="SaveChangesAsync"/>).</summary>
        Task AddAsync(Message message, CancellationToken cancellationToken);

        /// <summary>Bekleyen değişiklikleri veritabanına yazar.</summary>
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
