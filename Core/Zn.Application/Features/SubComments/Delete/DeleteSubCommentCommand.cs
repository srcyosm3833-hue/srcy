using System;

namespace Zn.Application.Features.SubComments.Delete
{
    /// <summary>
    /// Bir alt yorumu silme komutu. Alt yorumun SAHİBİ VEYA Admin rolündeki kullanıcı silebilir
    /// (aksi halde Forbidden 403). Alt yorum yoksa NotFound (404). Başarıda değer taşımayan sonuç
    /// döner (HTTP 204).
    /// <para>
    /// <see cref="RequestingUserId"/> ve <see cref="IsAdmin"/> istek gövdesinden DEĞİL,
    /// controller'da access token'dan doldurulur.
    /// </para>
    /// </summary>
    /// <param name="Id">Silinecek alt yorumun kimliği.</param>
    /// <param name="RequestingUserId">İsteği yapan kullanıcının kimliği — token'dan doldurulur.</param>
    /// <param name="IsAdmin">İsteği yapan kullanıcı Admin mi — token'daki rolden doldurulur.</param>
    public sealed record DeleteSubCommentCommand(Guid Id, string RequestingUserId, bool IsAdmin);
}
