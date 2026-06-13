namespace Zn.Application.Features.Users.GetUsers
{
    /// <summary>
    /// Admin/Manager kullanıcı yönetimi için kullanıcıları sayfalı getiren sorgu (A6 yetki matrisi:
    /// listeleme Admin + Manager). Başarıda <see cref="Common.UserResponse"/> öğelerinden oluşan bir
    /// <see cref="Zn.Application.Common.Pagination.PagedResult{T}"/> döner.
    /// <para>
    /// Sıralama: kayıt tarihine göre azalan (yeni kullanıcılar önce); sıralama ve sayfalama DB
    /// tarafında uygulanır. Sayfalama parametreleri handler içinde güvenli aralığa çekilir
    /// (page ≥ 1, 1 ≤ pageSize ≤ <see cref="MaxPageSize"/>).
    /// </para>
    /// <para>
    /// <paramref name="IncludeDeleted"/> true ise soft delete edilmiş kullanıcılar da listelenir
    /// (yalnızca Admin/Manager sorgusu). false (varsayılan) yalnızca aktif kullanıcıları döndürür.
    /// </para>
    /// </summary>
    /// <param name="Page">1 tabanlı sayfa numarası (varsayılan 1).</param>
    /// <param name="PageSize">Sayfa başına öğe sayısı (varsayılan 20, üst sınır <see cref="MaxPageSize"/>).</param>
    /// <param name="IncludeDeleted">Soft delete edilmiş kullanıcıların da dahil edilip edilmeyeceği.</param>
    public sealed record GetUsersQuery(int Page = 1, int PageSize = 20, bool IncludeDeleted = false)
    {
        /// <summary>İzin verilen azami sayfa boyutu. İstemci bunu aşan değer isterse buna sabitlenir.</summary>
        public const int MaxPageSize = 50;

        /// <summary>Varsayılan sayfa boyutu (istemci 0/negatif verirse kullanılır).</summary>
        public const int DefaultPageSize = 20;
    }
}
