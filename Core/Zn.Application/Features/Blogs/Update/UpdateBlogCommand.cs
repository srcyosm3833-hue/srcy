using System;

namespace Zn.Application.Features.Blogs.Update
{
    /// <summary>
    /// Var olan bir blogu güncelleme komutu. Yalnızca blogun yazarı VEYA Admin rolündeki
    /// kullanıcı güncelleyebilir; aksi halde Forbidden (403). Blog yoksa NotFound (404);
    /// hedef kategori yoksa Validation (400). Başarıda güncel <see cref="Common.BlogDetailResponse"/> döner.
    /// <para>
    /// <see cref="RequestingUserId"/> ve <see cref="IsAdmin"/> istek gövdesinden DEĞİL,
    /// controller'da access token'dan (ClaimsPrincipal) doldurulur.
    /// </para>
    /// </summary>
    /// <param name="Id">Güncellenecek blogun kimliği (route'tan gelir).</param>
    /// <param name="Title">Yeni başlık.</param>
    /// <param name="Description">Yeni içerik.</param>
    /// <param name="CoverImage">Yeni kapak görseli URL'i.</param>
    /// <param name="BlogImage">Yeni içerik görseli URL'i.</param>
    /// <param name="CategoryId">Yeni kategori kimliği (var olmalı).</param>
    /// <param name="RequestingUserId">İsteği yapan kullanıcının kimliği — token'dan doldurulur.</param>
    /// <param name="IsAdmin">İsteği yapan kullanıcı Admin mi — token'daki rolden doldurulur.</param>
    public sealed record UpdateBlogCommand(
        Guid Id,
        string Title,
        string Description,
        string CoverImage,
        string BlogImage,
        Guid CategoryId,
        string RequestingUserId,
        bool IsAdmin);
}
