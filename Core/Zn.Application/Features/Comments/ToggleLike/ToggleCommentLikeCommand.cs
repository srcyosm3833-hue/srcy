using System;

namespace Zn.Application.Features.Comments.ToggleLike
{
    /// <summary>
    /// Bir yorumun beğenisini açıp kapatan (toggle) komut. Giriş yapmış her kullanıcı kullanabilir.
    /// <para>
    /// Mevcut beğeni varsa kaldırılır (unlike), yoksa eklenir (like). İşlem idempotenttir; aynı
    /// kullanıcının eş zamanlı iki isteği composite PK (CommentId, UserId) sayesinde duplicate üretmez.
    /// <see cref="UserId"/> istek gövdesinden DEĞİL, controller'da access token'daki kimlikten
    /// (ClaimsPrincipal) doldurulur. Başarıda <see cref="Common.CommentLikeToggleResponse"/> döner;
    /// yorum yoksa NotFound (404).
    /// </para>
    /// </summary>
    /// <param name="CommentId">Beğenisi toggle edilecek yorumun kimliği (zorunlu).</param>
    /// <param name="UserId">Beğeniyi yapan kullanıcının kimliği — token'dan doldurulur, gövdeden alınmaz.</param>
    public sealed record ToggleCommentLikeCommand(Guid CommentId, string UserId);
}
