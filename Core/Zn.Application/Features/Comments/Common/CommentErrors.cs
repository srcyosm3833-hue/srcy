using System;
using Zn.Application.Common.Results;

namespace Zn.Application.Features.Comments.Common
{
    /// <summary>
    /// Comment dikey diliminde tekrar eden hata tanımlarını tek noktada toplar.
    /// Handler'lar bu fabrikalar üzerinden anlamlı <see cref="Error"/> üretir;
    /// ApiControllerBase bunları uygun HTTP koduna eşler.
    /// </summary>
    public static class CommentErrors
    {
        /// <summary>Belirtilen Id'ye sahip yorum bulunamadı (404).</summary>
        public static Error NotFound(Guid id) =>
            Error.NotFound("Comment.NotFound", $"Comment with id '{id}' was not found.");

        /// <summary>Yorumun yapılmak/listelenmek istendiği blog mevcut değil (404).</summary>
        public static Error BlogNotFound(Guid blogId) =>
            Error.NotFound("Comment.BlogNotFound", $"Blog with id '{blogId}' was not found.");

        /// <summary>
        /// Kullanıcı bu yorumu düzenleme yetkisine sahip değil (403).
        /// Düzenleme yalnızca yorumun sahibine açıktır; Admin bile başkasının yorumunu düzenleyemez.
        /// </summary>
        public static Error ForbiddenEdit() =>
            Error.Forbidden(
                "Comment.ForbiddenEdit",
                "You are not allowed to edit this comment. Only its author can.");

        /// <summary>
        /// Kullanıcı bu yorumu silme yetkisine sahip değil (403).
        /// Silme yorumun sahibine veya bir Admin'e açıktır.
        /// </summary>
        public static Error ForbiddenDelete() =>
            Error.Forbidden(
                "Comment.ForbiddenDelete",
                "You are not allowed to delete this comment. Only its author or an administrator can.");
    }
}
