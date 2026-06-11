using System;
using Zn.Application.Common.Results;

namespace Zn.Application.Features.SubComments.Common
{
    /// <summary>
    /// SubComment dikey diliminde tekrar eden hata tanımlarını tek noktada toplar.
    /// Handler'lar bu fabrikalar üzerinden anlamlı <see cref="Error"/> üretir;
    /// ApiControllerBase bunları uygun HTTP koduna eşler.
    /// </summary>
    public static class SubCommentErrors
    {
        /// <summary>Belirtilen Id'ye sahip alt yorum bulunamadı (404).</summary>
        public static Error NotFound(Guid id) =>
            Error.NotFound("SubComment.NotFound", $"Sub-comment with id '{id}' was not found.");

        /// <summary>Alt yorumun ekleneceği ana yorum mevcut değil (404).</summary>
        public static Error CommentNotFound(Guid commentId) =>
            Error.NotFound("SubComment.CommentNotFound", $"Comment with id '{commentId}' was not found.");

        /// <summary>
        /// Kullanıcı bu alt yorumu düzenleme yetkisine sahip değil (403).
        /// Düzenleme yalnızca alt yorumun sahibine açıktır; Admin bile düzenleyemez.
        /// </summary>
        public static Error ForbiddenEdit() =>
            Error.Forbidden(
                "SubComment.ForbiddenEdit",
                "You are not allowed to edit this sub-comment. Only its author can.");

        /// <summary>
        /// Kullanıcı bu alt yorumu silme yetkisine sahip değil (403).
        /// Silme alt yorumun sahibine veya bir Admin'e açıktır.
        /// </summary>
        public static Error ForbiddenDelete() =>
            Error.Forbidden(
                "SubComment.ForbiddenDelete",
                "You are not allowed to delete this sub-comment. Only its author or an administrator can.");
    }
}
