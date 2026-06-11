using System;

namespace Zn.Application.Features.Comments.Add
{
    /// <summary>
    /// Bir bloga yorum ekleme komutu. Giriş yapmış her kullanıcı yorum yapabilir.
    /// <para>
    /// <see cref="UserId"/> istek gövdesinden DEĞİL, controller'da access token'daki kimlikten
    /// (ClaimsPrincipal) doldurulur; böylece bir kullanıcı başka biri adına yorum yapamaz.
    /// Blog yoksa NotFound (404). Başarıda oluşturulan yorumun <see cref="Common.CommentResponse"/>'u döner.
    /// </para>
    /// </summary>
    /// <param name="BlogId">Yorumun yapılacağı blogun kimliği (route'tan gelir).</param>
    /// <param name="CommentText">Yorum içeriği (zorunlu).</param>
    /// <param name="UserId">Yorumu yapan kullanıcının kimliği — token'dan doldurulur, gövdeden alınmaz.</param>
    public sealed record AddCommentCommand(Guid BlogId, string CommentText, string UserId);
}
