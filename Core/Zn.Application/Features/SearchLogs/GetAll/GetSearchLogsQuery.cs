namespace Zn.Application.Features.SearchLogs.GetAll
{
    /// <summary>
    /// Admin arama log paneli için arama loglarını sayfalı ve (opsiyonel) terim filtreli getiren
    /// sorgu. Yalnızca Admin erişebilir (kişisel veri riski; Manager DEĞİL). Başarıda
    /// <see cref="Common.SearchLogResponse"/> öğelerinden oluşan bir
    /// <see cref="Zn.Application.Common.Pagination.PagedResult{T}"/> döner.
    /// <para>
    /// Sıralama: SearchedAt azalan (en yeni önce), DB tarafında. Sayfalama parametreleri handler
    /// içinde güvenli aralığa çekilir (page ≥ 1, 1 ≤ pageSize ≤ <see cref="MaxPageSize"/>).
    /// <paramref name="Term"/> verilirse Term alanında büyük/küçük harf duyarsız LIKE filtresi uygulanır.
    /// </para>
    /// </summary>
    /// <param name="Page">1 tabanlı sayfa numarası (varsayılan 1).</param>
    /// <param name="PageSize">Sayfa başına öğe sayısı (varsayılan 20, üst sınır <see cref="MaxPageSize"/>).</param>
    /// <param name="Term">Verilirse yalnızca terimi içeren loglar döner (opsiyonel filtre).</param>
    public sealed record GetSearchLogsQuery(int Page = 1, int PageSize = 20, string? Term = null)
    {
        /// <summary>İzin verilen azami sayfa boyutu. İstemci bunu aşan değer isterse buna sabitlenir.</summary>
        public const int MaxPageSize = 100;

        /// <summary>Varsayılan sayfa boyutu (istemci 0/negatif verirse kullanılır).</summary>
        public const int DefaultPageSize = 20;
    }
}
