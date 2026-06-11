namespace Zn.Application.Features.Messages.GetAll
{
    /// <summary>
    /// Yönetici mesaj kutusu için mesajları sayfalı getiren sorgu. Yalnızca Admin erişebilir.
    /// Başarıda <see cref="Common.MessageResponse"/> öğelerinden oluşan bir
    /// <see cref="Zn.Application.Common.Pagination.PagedResult{T}"/> döner.
    /// <para>
    /// Sıralama: önce okunmamış mesajlar (IsRead=false), ardından her grup içinde CreatedAt azalan;
    /// böylece dikkat gerektiren mesajlar listenin başında çıkar. Sıralama ve sayfalama DB tarafında
    /// uygulanır. Sayfalama parametreleri handler içinde güvenli aralığa çekilir
    /// (page ≥ 1, 1 ≤ pageSize ≤ <see cref="MaxPageSize"/>).
    /// </para>
    /// </summary>
    /// <param name="Page">1 tabanlı sayfa numarası (varsayılan 1).</param>
    /// <param name="PageSize">Sayfa başına öğe sayısı (varsayılan 10, üst sınır <see cref="MaxPageSize"/>).</param>
    public sealed record GetMessagesQuery(int Page = 1, int PageSize = 10)
    {
        /// <summary>İzin verilen azami sayfa boyutu. İstemci bunu aşan değer isterse buna sabitlenir.</summary>
        public const int MaxPageSize = 50;

        /// <summary>Varsayılan sayfa boyutu (istemci 0/negatif verirse kullanılır).</summary>
        public const int DefaultPageSize = 10;
    }
}
