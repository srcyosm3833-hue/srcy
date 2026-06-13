using System;

namespace Zn.Application.Features.Blogs.Search
{
    /// <summary>
    /// Blogları serbest metin (<paramref name="Q"/>) üzerinde başlık ve açıklamada arayan,
    /// sayfalı ve (opsiyonel) kategori filtreli sorgu. Herkese açıktır (anonim dahil).
    /// Başarıda <see cref="Common.BlogListItemResponse"/> öğelerinden oluşan bir
    /// <see cref="Zn.Application.Common.Pagination.PagedResult{T}"/> döner.
    /// <para>
    /// Soft delete edilmiş bloglar global query filter (IsDeleted == false) sayesinde
    /// arama sonuçlarından otomatik dışlanır; bu sorgu yalnızca public davranışı sunar.
    /// Sayfalama parametrelerinin sınırları <see cref="SearchBlogsQueryValidator"/> ile
    /// doğrulanır (page ≥ 1, 1 ≤ pageSize ≤ <see cref="MaxPageSize"/>).
    /// </para>
    /// </summary>
    /// <param name="Q">Aranacak terim (başlık VEYA açıklamada eşleşir). Boş/whitespace olamaz.</param>
    /// <param name="Page">1 tabanlı sayfa numarası (varsayılan 1).</param>
    /// <param name="PageSize">Sayfa başına öğe sayısı (varsayılan 10, üst sınır <see cref="MaxPageSize"/>).</param>
    /// <param name="CategoryId">Verilirse yalnızca bu kategoriye ait bloglar içinde aranır.</param>
    /// <param name="CurrentUserId">
    /// İsteği yapan kullanıcının kimliği — token'dan doldurulur, gövdeden alınmaz. Verilirse her
    /// blog için "bu kullanıcı beğendi mi" (IsLikedByCurrentUser) DB'de hesaplanır; anonimde null
    /// olur ve IsLikedByCurrentUser false döner.
    /// </param>
    public sealed record SearchBlogsQuery(
        string Q,
        int Page = 1,
        int PageSize = 10,
        Guid? CategoryId = null,
        string? CurrentUserId = null)
    {
        /// <summary>İzin verilen azami sayfa boyutu.</summary>
        public const int MaxPageSize = 50;

        /// <summary>Varsayılan sayfa boyutu.</summary>
        public const int DefaultPageSize = 10;

        /// <summary>Aranan terimin azami uzunluğu.</summary>
        public const int QueryMaxLength = 200;
    }
}
