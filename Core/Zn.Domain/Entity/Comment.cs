using System;
using System.Collections.Generic;
using Zn.Domain.Entity.Common;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// Blog yazısına yapılan yorumu temsil eder. BaseEntity'den
    /// Guid tipinde Id, CreatedAt ve UpdatedAt alanlarını miras alır.
    /// </summary>
    public class Comment : BaseEntity
    {
        /// <summary>Yorum içeriği. Non-nullable.</summary>
        public string CommentText { get; set; } = null!;

        /// <summary>Foreign key: Yorumun yapıldığı blogun Id'si.</summary>
        public Guid BlogId { get; set; }

        /// <summary>Navigation property: Yorumun yapıldığı blog (N Comment - 1 Blog).</summary>
        public Blog Blog { get; set; } = null!;

        /// <summary>
        /// Foreign key: Yorumu yapan kullanıcının Id'si.
        /// IdentityUser'ın anahtar tipi string olduğu için string'dir.
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>Navigation property: Yorumu yapan kullanıcı (N Comment - 1 User).</summary>
        public User User { get; set; } = null!;

        /// <summary>Navigation property: Yoruma verilen yanıtlar (1 Comment - N SubComment).</summary>
        public ICollection<SubComment> SubComments { get; set; } = new List<SubComment>();
    }
}
