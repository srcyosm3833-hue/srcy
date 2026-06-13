namespace Zn.Application.Features.Comments.GetAllForAdmin
{
    /// <summary>
    /// Tüm bloglardaki yorumları VE alt yorumları tek bir DÜZ (flat) moderasyon listesinde, sayfalı
    /// (createdAt azalan — en yeni üstte) getiren admin sorgusu. Yalnızca Admin'e açıktır (route
    /// seviyesinde <c>[Authorize(Roles = RoleNames.Admin)]</c>).
    /// <para>
    /// Başarıda <see cref="CommentModerationResponse"/> öğelerinden oluşan bir
    /// <see cref="Zn.Application.Common.Pagination.PagedResult{T}"/> döner. Sayfalama parametreleri
    /// handler içinde güvenli aralığa çekilir (page ≥ 1, 1 ≤ pageSize ≤ <see cref="MaxPageSize"/>).
    /// </para>
    /// <para>
    /// Soft delete: Comment ve SubComment entity'lerinde global query filter (silinmiş blogun
    /// yorumları + silinmiş kullanıcının alt yorumları otomatik dışlanır) etkindir ve bilinçli olarak
    /// bypass EDİLMEZ — moderasyon listesi yalnızca aktif kayıtları gösterir (scope basit tutulur).
    /// </para>
    /// </summary>
    /// <param name="Page">1 tabanlı sayfa numarası (varsayılan 1).</param>
    /// <param name="PageSize">Sayfa başına öğe sayısı (varsayılan 20, üst sınır <see cref="MaxPageSize"/>).</param>
    public sealed record GetCommentsForAdminQuery(
        int Page = 1,
        int PageSize = 20)
    {
        /// <summary>İzin verilen azami sayfa boyutu. İstemci bunu aşan değer isterse buna sabitlenir.</summary>
        public const int MaxPageSize = 100;

        /// <summary>Varsayılan sayfa boyutu (istemci 0/negatif verirse kullanılır).</summary>
        public const int DefaultPageSize = 20;
    }
}
