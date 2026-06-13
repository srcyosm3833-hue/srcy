using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Features.SearchLogs.Common;
using Zn.Domain.Entity;

namespace Zn.Application.Interfaces.Persistence
{
    /// <summary>
    /// SearchLog kalıcılaştırma sözleşmesi. İmplementasyon Zn.Persistence'tadır; Application
    /// katmanı EF Core'a doğrudan bağımlı olmadan arama loglarını yazar ve listeler.
    /// <para>
    /// Yazma (<see cref="AddAsync"/>) kaydı hemen kalıcılaştırır: arama loglama "ateşle-unut"
    /// yan etki olduğundan kendi SaveChanges'ini kapsar (çağıran ayrı bir Unit of Work tutmaz).
    /// Listeleme (<see cref="GetPagedAsync"/>) AsNoTracking + DB seviyesinde projeksiyon + tarih
    /// azalan sıralama kullanır.
    /// </para>
    /// </summary>
    public interface ISearchLogRepository
    {
        /// <summary>
        /// Yeni bir arama log kaydını ekler ve hemen kalıcılaştırır. Bu metodun başarısız olması
        /// asıl aramayı bloklamamalıdır; çağıran handler try/catch ile sarmalar.
        /// </summary>
        Task AddAsync(SearchLog searchLog, CancellationToken cancellationToken);

        /// <summary>
        /// Arama loglarını <see cref="SearchLog.SearchedAt"/> azalan (en yeni önce) sıralı,
        /// sayfalanmış olarak ve toplam sayısıyla birlikte döner. Yalnızca okuma amaçlıdır
        /// (AsNoTracking + DB seviyesinde projeksiyon).
        /// <para>
        /// <paramref name="term"/> verilirse (null/boş değilse) yalnızca <see cref="SearchLog.Term"/>
        /// alanı bu değeri içeren kayıtlar döner (büyük/küçük harf duyarsız LIKE).
        /// </para>
        /// </summary>
        /// <returns>Geçerli sayfadaki log öğeleri ve filtreye uyan toplam kayıt sayısı.</returns>
        Task<(IReadOnlyList<SearchLogResponse> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            string? term,
            CancellationToken cancellationToken);
    }
}
