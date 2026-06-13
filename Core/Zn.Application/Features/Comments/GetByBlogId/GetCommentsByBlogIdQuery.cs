using System;

namespace Zn.Application.Features.Comments.GetByBlogId
{
    /// <summary>
    /// Bir bloga ait yorumları sayfalı (createdAt azalan) getiren sorgu. Herkese açıktır.
    /// Başarıda <see cref="Common.CommentResponse"/> öğelerinden oluşan bir
    /// <see cref="Zn.Application.Common.Pagination.PagedResult{T}"/> döner. Blog yoksa NotFound (404).
    /// <para>
    /// Sayfalama parametreleri handler içinde güvenli aralığa çekilir (page ≥ 1,
    /// 1 ≤ pageSize ≤ <see cref="MaxPageSize"/>); böylece istemci aşırı büyük sayfa
    /// boyutu isteyerek veritabanını yoramaz.
    /// </para>
    /// </summary>
    /// <param name="BlogId">Yorumları getirilecek blogun kimliği.</param>
    /// <param name="Page">1 tabanlı sayfa numarası (varsayılan 1).</param>
    /// <param name="PageSize">Sayfa başına öğe sayısı (varsayılan 10, üst sınır <see cref="MaxPageSize"/>).</param>
    /// <param name="CurrentUserId">
    /// İsteği yapan kullanıcının kimliği — token'dan doldurulur, gövdeden alınmaz. Verilirse her
    /// yorum için "bu kullanıcı beğendi mi" (IsLikedByCurrentUser) DB'de hesaplanır; anonimde null
    /// olur ve IsLikedByCurrentUser false döner.
    /// </param>
    public sealed record GetCommentsByBlogIdQuery(
        Guid BlogId,
        int Page = 1,
        int PageSize = 10,
        string? CurrentUserId = null)
    {
        /// <summary>İzin verilen azami sayfa boyutu. İstemci bunu aşan değer isterse buna sabitlenir.</summary>
        public const int MaxPageSize = 50;

        /// <summary>Varsayılan sayfa boyutu (istemci 0/negatif verirse kullanılır).</summary>
        public const int DefaultPageSize = 10;
    }
}
