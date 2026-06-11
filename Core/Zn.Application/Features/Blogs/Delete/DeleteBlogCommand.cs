using System;

namespace Zn.Application.Features.Blogs.Delete
{
    /// <summary>
    /// Bir blogu silme komutu. Yalnızca blogun yazarı VEYA Admin rolündeki kullanıcı silebilir;
    /// aksi halde Forbidden (403). Blog yoksa NotFound (404). Başarıda değer taşımayan bir sonuç
    /// döner (HTTP 204).
    /// <para>
    /// <see cref="RequestingUserId"/> ve <see cref="IsAdmin"/> istek gövdesinden DEĞİL,
    /// controller'da access token'dan doldurulur.
    /// </para>
    /// </summary>
    /// <param name="Id">Silinecek blogun kimliği.</param>
    /// <param name="RequestingUserId">İsteği yapan kullanıcının kimliği — token'dan doldurulur.</param>
    /// <param name="IsAdmin">İsteği yapan kullanıcı Admin mi — token'daki rolden doldurulur.</param>
    public sealed record DeleteBlogCommand(Guid Id, string RequestingUserId, bool IsAdmin);
}
