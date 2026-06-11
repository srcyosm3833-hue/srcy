using System;
using System.Collections.Generic;
using Zn.Domain.Entity.Common;
using Zn.Domain.Exceptions;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// Blog kategorisini temsil eder. BaseEntity'den Guid tipinde Id,
    /// CreatedAt ve UpdatedAt alanlarını miras alır.
    /// <para>
    /// Invariant'lar (boş olmayan, azami uzunluğu aşmayan ad) factory metodu
    /// <see cref="Create"/> ve mutator <see cref="Rename"/> içinde korunur; geçersiz
    /// durumda <see cref="CategoryDomainException"/> fırlatılır. Bu sayede geçersiz bir
    /// Category nesnesi hiçbir zaman var olamaz.
    /// </para>
    /// </summary>
    public class Category : BaseEntity
    {
        /// <summary>
        /// Kategori adının azami uzunluğu. CategoryConfiguration'daki HasMaxLength(100)
        /// ile birebir aynıdır; veritabanı kısıtı ile domain invariant'ı senkron tutulur.
        /// </summary>
        public const int CategoryNameMaxLength = 100;

        /// <summary>
        /// EF Core materyalizasyonu için parametresiz constructor.
        /// Uygulama kodu yerine <see cref="Create"/> factory'sini kullanmalıdır.
        /// </summary>
        private Category()
        {
        }

        /// <summary>
        /// Kategori adı. Dışarıdan yalnızca okunabilir; değişiklik <see cref="Rename"/>
        /// üzerinden invariant kontrolüyle yapılır.
        /// </summary>
        public string CategoryName { get; private set; } = null!;

        /// <summary>
        /// Navigation property: Bu kategoriye ait bloglar (1 Kategori - N Blog).
        /// Boş liste ile başlatılır ki null kontrolü gerektirmeden üzerinde gezinilebilsin.
        /// </summary>
        public ICollection<Blog> Blogs { get; private set; } = new List<Blog>();

        /// <summary>
        /// Geçerli bir Category oluşturur. Ad boş/whitespace olamaz ve
        /// <see cref="CategoryNameMaxLength"/>'i aşamaz; aksi halde
        /// <see cref="CategoryDomainException"/> fırlatılır.
        /// </summary>
        /// <param name="categoryName">Kategori adı (trim edilerek saklanır).</param>
        public static Category Create(string categoryName)
        {
            string normalized = Normalize(categoryName);

            return new Category
            {
                Id = Guid.NewGuid(),
                CategoryName = normalized,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Kategori adını invariant kontrolüyle değiştirir ve <see cref="BaseEntity{TId}.UpdatedAt"/>'i günceller.
        /// Geçersiz ad <see cref="CategoryDomainException"/> fırlatır.
        /// </summary>
        public void Rename(string categoryName)
        {
            CategoryName = Normalize(categoryName);
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>Ad invariant'larını uygular: trim + boşluk + uzunluk kontrolü.</summary>
        private static string Normalize(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new CategoryDomainException("Category name cannot be empty.");
            }

            string trimmed = categoryName.Trim();

            if (trimmed.Length > CategoryNameMaxLength)
            {
                throw new CategoryDomainException(
                    $"Category name cannot exceed {CategoryNameMaxLength} characters.");
            }

            return trimmed;
        }
    }
}
