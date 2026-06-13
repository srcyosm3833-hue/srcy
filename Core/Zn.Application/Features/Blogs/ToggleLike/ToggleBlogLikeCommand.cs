using System;

namespace Zn.Application.Features.Blogs.ToggleLike
{
    /// <summary>
    /// Bir blogun beğenisini açıp kapatan (toggle) komut. Giriş yapmış her kullanıcı kullanabilir.
    /// <para>
    /// Mevcut beğeni varsa kaldırılır (unlike), yoksa eklenir (like). İşlem idempotenttir; aynı
    /// kullanıcının eş zamanlı iki isteği composite PK (BlogId, UserId) sayesinde duplicate üretmez.
    /// <see cref="UserId"/> istek gövdesinden DEĞİL, controller'da access token'daki kimlikten
    /// (ClaimsPrincipal) doldurulur. Başarıda <see cref="Common.BlogLikeToggleResponse"/> döner;
    /// blog yoksa NotFound (404).
    /// </para>
    /// </summary>
    /// <param name="BlogId">Beğenisi toggle edilecek blogun kimliği (zorunlu).</param>
    /// <param name="UserId">Beğeniyi yapan kullanıcının kimliği — token'dan doldurulur, gövdeden alınmaz.</param>
    public sealed record ToggleBlogLikeCommand(Guid BlogId, string UserId);
}
