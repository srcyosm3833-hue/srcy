using System;

namespace Zn.Application.Features.SubComments.Add
{
    /// <summary>
    /// Bir yoruma yanıt (alt yorum) ekleme komutu. Giriş yapmış her kullanıcı yanıt verebilir.
    /// <para>
    /// <see cref="UserId"/> istek gövdesinden DEĞİL, controller'da access token'daki kimlikten
    /// (ClaimsPrincipal) doldurulur. Ana yorum yoksa NotFound (404). Başarıda oluşturulan alt
    /// yorumun <see cref="Common.SubCommentResponse"/>'u döner.
    /// </para>
    /// </summary>
    /// <param name="CommentId">Yanıtlanacak ana yorumun kimliği (route'tan gelir).</param>
    /// <param name="SubCommentText">Alt yorum içeriği (zorunlu).</param>
    /// <param name="UserId">Alt yorumu yapan kullanıcının kimliği — token'dan doldurulur, gövdeden alınmaz.</param>
    public sealed record AddSubCommentCommand(Guid CommentId, string SubCommentText, string UserId);
}
