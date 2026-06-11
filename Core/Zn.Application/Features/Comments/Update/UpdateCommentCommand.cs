using System;

namespace Zn.Application.Features.Comments.Update
{
    /// <summary>
    /// Var olan bir yorumu güncelleme komutu. Yalnızca yorumun SAHİBİ düzenleyebilir; Admin bile
    /// başkasının yorumunu düzenleyemez (aksi halde Forbidden 403). Yorum yoksa NotFound (404).
    /// Başarıda güncel <see cref="Common.CommentResponse"/> döner (isEdited: true).
    /// <para>
    /// <see cref="RequestingUserId"/> istek gövdesinden DEĞİL, controller'da access token'dan
    /// (ClaimsPrincipal) doldurulur.
    /// </para>
    /// </summary>
    /// <param name="Id">Güncellenecek yorumun kimliği (route'tan gelir).</param>
    /// <param name="CommentText">Yeni yorum içeriği (zorunlu).</param>
    /// <param name="RequestingUserId">İsteği yapan kullanıcının kimliği — token'dan doldurulur.</param>
    public sealed record UpdateCommentCommand(Guid Id, string CommentText, string RequestingUserId);
}
