using System;

namespace Zn.Application.Features.Blogs.Create
{
    /// <summary>
    /// Yeni blog oluşturma komutu. Giriş yapmış her kullanıcı yazar olabilir.
    /// <para>
    /// <see cref="UserId"/> istek gövdesinden DEĞİL, controller'da access token'daki
    /// kimlikten (ClaimsPrincipal) doldurulur; böylece bir kullanıcı başka biri adına
    /// blog oluşturamaz. Başarıda oluşturulan blogun <see cref="Common.BlogDetailResponse"/>'u döner.
    /// </para>
    /// </summary>
    /// <param name="Title">Blog başlığı (zorunlu).</param>
    /// <param name="Description">Blog içeriği (zorunlu).</param>
    /// <param name="CoverImage">Kapak görseli URL'i (zorunlu).</param>
    /// <param name="BlogImage">İçerik görseli URL'i (zorunlu).</param>
    /// <param name="CategoryId">Bağlanacak kategori kimliği (zorunlu, var olmalı).</param>
    /// <param name="UserId">Yazarın kullanıcı kimliği — token'dan doldurulur, gövdeden alınmaz.</param>
    public sealed record CreateBlogCommand(
        string Title,
        string Description,
        string CoverImage,
        string BlogImage,
        Guid CategoryId,
        string UserId);
}
