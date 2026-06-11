using System;
using System.Collections.Generic;
using Zn.Domain.Entity.Common;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// Blog yazısını temsil eder. BaseEntity'den Guid tipinde Id,
    /// CreatedAt ve UpdatedAt alanlarını miras alır.
    /// </summary>
    public class Blog : BaseEntity
    {
        /// <summary>Blog başlığı. Non-nullable.</summary>
        public string Title { get; set; } = null!;

        /// <summary>Kapak görseli yolu/URL'i. Non-nullable.</summary>
        public string CoverImage { get; set; } = null!;

        /// <summary>Blog içerik görseli yolu/URL'i. Non-nullable.</summary>
        public string BlogImage { get; set; } = null!;

        /// <summary>Blog açıklaması/içeriği. Non-nullable.</summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// Foreign key: Bağlı olduğu kategorinin Id'si.
        /// Guid value-type olduğu için zaten non-nullable'dır; her blog bir kategoriye ait olmak zorundadır.
        /// </summary>
        public Guid CategoryId { get; set; }

        /// <summary>
        /// Navigation property: Blogun ait olduğu kategori (N Blog - 1 Kategori).
        /// Non-nullable; ilişki zorunluluğu Fluent API tarafında IsRequired ile tanımlanır.
        /// </summary>
        public Category Category { get; set; } = null!;

        /// <summary>
        /// Foreign key: Blogu yazan kullanıcının Id'si.
        /// IdentityUser'ın anahtar tipi string olduğu için string'dir.
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>Navigation property: Blogu yazan kullanıcı (N Blog - 1 User).</summary>
        public User User { get; set; } = null!;

        /// <summary>Navigation property: Bloga yapılan yorumlar (1 Blog - N Comment).</summary>
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
