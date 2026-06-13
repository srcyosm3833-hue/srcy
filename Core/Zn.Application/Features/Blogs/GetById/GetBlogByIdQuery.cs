using System;

namespace Zn.Application.Features.Blogs.GetById
{
    /// <summary>
    /// Tek bir blogu Id ile tam detayıyla getiren sorgu (herkese açık).
    /// Bulunamazsa NotFound (404). Başarıda <see cref="Common.BlogDetailResponse"/> döner.
    /// </summary>
    /// <param name="Id">Getirilecek blogun kimliği.</param>
    /// <param name="CurrentUserId">
    /// İsteği yapan kullanıcının kimliği — token'dan doldurulur, gövdeden alınmaz. Verilirse
    /// "bu kullanıcı beğendi mi" (IsLikedByCurrentUser) DB'de hesaplanır; anonimde null olur ve
    /// IsLikedByCurrentUser false döner.
    /// </param>
    public sealed record GetBlogByIdQuery(Guid Id, string? CurrentUserId = null);
}
