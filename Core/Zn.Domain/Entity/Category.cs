using System;
using System.Collections.Generic;
using Zn.Domain.Entity.Common;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// Blog kategorisini temsil eder. BaseEntity'den Guid tipinde Id,
    /// CreatedAt ve UpdatedAt alanlarını miras alır.
    /// </summary>
    public class Category : BaseEntity
    {
        /// <summary>
        /// Kategori adı. Non-nullable; EF Core materyalizasyonu için null! ile başlatılır,
        /// veritabanı zorunluluğu Fluent API tarafında IsRequired ile garanti edilir.
        /// </summary>
        public string CategoryName { get; set; } = null!;

        /// <summary>
        /// Navigation property: Bu kategoriye ait bloglar (1 Kategori - N Blog).
        /// Boş liste ile başlatılır ki null kontrolü gerektirmeden üzerinde gezinilebilsin.
        /// </summary>
        public ICollection<Blog> Blogs { get; set; } = new List<Blog>();
    }
}
