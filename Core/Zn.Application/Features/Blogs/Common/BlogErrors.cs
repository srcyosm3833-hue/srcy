using System;
using System.Collections.Generic;
using Zn.Application.Common.Results;

namespace Zn.Application.Features.Blogs.Common
{
    /// <summary>
    /// Blog dikey diliminde tekrar eden hata tanımlarını tek noktada toplar.
    /// Handler'lar bu fabrikalar üzerinden anlamlı <see cref="Error"/> üretir;
    /// ApiControllerBase bunları uygun HTTP koduna eşler.
    /// </summary>
    public static class BlogErrors
    {
        /// <summary>Belirtilen Id'ye sahip blog bulunamadı (404).</summary>
        public static Error NotFound(Guid id) =>
            Error.NotFound("Blog.NotFound", $"Blog with id '{id}' was not found.");

        /// <summary>
        /// Verilen kategori mevcut değil (400). Create/Update'te seçilen kategori
        /// veritabanında yoksa döner; alan bazlı doğrulama hatası olarak sunulur.
        /// </summary>
        public static Error CategoryNotFound(Guid categoryId) =>
            Error.Validation(
                "Blog.CategoryNotFound",
                $"Category with id '{categoryId}' does not exist.",
                new Dictionary<string, string[]>
                {
                    ["categoryId"] = new[] { $"Category with id '{categoryId}' does not exist." }
                });

        /// <summary>
        /// Kullanıcı bu blog üzerinde değişiklik/silme yetkisine sahip değil (403).
        /// Yalnızca blogun yazarı veya Admin rolündeki kullanıcılar yetkilidir.
        /// </summary>
        public static Error Forbidden() =>
            Error.Forbidden(
                "Blog.Forbidden",
                "You are not allowed to modify this blog. Only the author or an administrator can.");
    }
}
