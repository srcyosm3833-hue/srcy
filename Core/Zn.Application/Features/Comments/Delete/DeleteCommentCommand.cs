using System;

namespace Zn.Application.Features.Comments.Delete
{
    /// <summary>
    /// Bir yorumu silme komutu. Yorumun SAHİBİ VEYA Admin rolündeki kullanıcı silebilir
    /// (aksi halde Forbidden 403). Yorum yoksa NotFound (404). Başarıda değer taşımayan sonuç
    /// döner (HTTP 204). Comment → SubComment ilişkisi Cascade olduğundan alt yorumlar
    /// veritabanı tarafından otomatik silinir.
    /// <para>
    /// <see cref="RequestingUserId"/> ve <see cref="IsAdmin"/> istek gövdesinden DEĞİL,
    /// controller'da access token'dan doldurulur.
    /// </para>
    /// </summary>
    /// <param name="Id">Silinecek yorumun kimliği.</param>
    /// <param name="RequestingUserId">İsteği yapan kullanıcının kimliği — token'dan doldurulur.</param>
    /// <param name="IsAdmin">İsteği yapan kullanıcı Admin mi — token'daki rolden doldurulur.</param>
    public sealed record DeleteCommentCommand(Guid Id, string RequestingUserId, bool IsAdmin);
}
