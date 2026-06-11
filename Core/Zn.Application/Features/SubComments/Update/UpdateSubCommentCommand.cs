using System;

namespace Zn.Application.Features.SubComments.Update
{
    /// <summary>
    /// Var olan bir alt yorumu güncelleme komutu. Yalnızca alt yorumun SAHİBİ düzenleyebilir;
    /// Admin bile başkasının alt yorumunu düzenleyemez (aksi halde Forbidden 403). Alt yorum yoksa
    /// NotFound (404). Başarıda güncel <see cref="Common.SubCommentResponse"/> döner (isEdited: true).
    /// <para>
    /// <see cref="RequestingUserId"/> istek gövdesinden DEĞİL, controller'da access token'dan
    /// (ClaimsPrincipal) doldurulur.
    /// </para>
    /// </summary>
    /// <param name="Id">Güncellenecek alt yorumun kimliği (route'tan gelir).</param>
    /// <param name="SubCommentText">Yeni alt yorum içeriği (zorunlu).</param>
    /// <param name="RequestingUserId">İsteği yapan kullanıcının kimliği — token'dan doldurulur.</param>
    public sealed record UpdateSubCommentCommand(Guid Id, string SubCommentText, string RequestingUserId);
}
