using System;
using Zn.Application.Common.Results;

namespace Zn.Application.Features.Categories.Common
{
    /// <summary>
    /// Category dikey diliminde tekrar eden hata tanımlarını tek noktada toplar.
    /// Handler'lar bu fabrikalar üzerinden anlamlı <see cref="Error"/> üretir;
    /// ApiControllerBase bunları uygun HTTP koduna eşler.
    /// </summary>
    public static class CategoryErrors
    {
        /// <summary>Belirtilen Id'ye sahip kategori bulunamadı (404).</summary>
        public static Error NotFound(Guid id) =>
            Error.NotFound("Category.NotFound", $"Category with id '{id}' was not found.");

        /// <summary>Aynı isimde bir kategori zaten mevcut (409).</summary>
        public static Error NameAlreadyExists(string categoryName) =>
            Error.Conflict(
                "Category.NameAlreadyExists",
                $"A category named '{categoryName}' already exists.");

        /// <summary>
        /// Kategori, kendisine bağlı bloglar bulunduğu için silinemez (409).
        /// Blog → Category FK'sı Restrict olduğundan veritabanı hatası yerine
        /// handler bu anlamlı çakışmayı önceden döndürür.
        /// </summary>
        public static Error HasBlogs(Guid id) =>
            Error.Conflict(
                "Category.HasBlogs",
                $"Category with id '{id}' cannot be deleted because it still has blogs assigned to it.");
    }
}
