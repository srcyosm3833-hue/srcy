using System;
using Zn.Domain.Entity.Common;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// Bir yoruma verilen yanıtı (alt yorum) temsil eder. BaseEntity'den
    /// Guid tipinde Id, CreatedAt ve UpdatedAt alanlarını miras alır.
    /// </summary>
    public class SubComment : BaseEntity
    {
        /// <summary>Alt yorum içeriği. Non-nullable.</summary>
        public string SubCommentText { get; set; } = null!;

        /// <summary>Foreign key: Yanıtlanan ana yorumun Id'si.</summary>
        public Guid CommentId { get; set; }

        /// <summary>Navigation property: Yanıtlanan ana yorum (N SubComment - 1 Comment).</summary>
        public Comment Comment { get; set; } = null!;

        /// <summary>
        /// Foreign key: Alt yorumu yapan kullanıcının Id'si.
        /// IdentityUser'ın anahtar tipi string olduğu için string'dir.
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>Navigation property: Alt yorumu yapan kullanıcı (N SubComment - 1 User).</summary>
        public User User { get; set; } = null!;
    }
}
