using System;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// Bir kullanıcının bir yorumu beğendiğini temsil eden saf ilişki (join) tablosu.
    /// <para>
    /// Karar: Surrogate Id YOKTUR; birincil anahtar composite olarak
    /// (<see cref="CommentId"/>, <see cref="UserId"/>) çiftidir. Bu sayede aynı kullanıcının
    /// aynı yorumu birden fazla kez beğenmesi veritabanı seviyesinde (PK constraint) engellenir
    /// ve beğeni doğal olarak idempotent olur. <see cref="Common.BaseEntity"/>'den miras ALMAZ —
    /// Id/equality davranışına ihtiyaç yoktur.
    /// </para>
    /// </summary>
    public class CommentLike
    {
        /// <summary>
        /// EF Core materyalizasyonu için parametresiz constructor.
        /// Uygulama kodu yerine <see cref="Create"/> factory'sini kullanmalıdır.
        /// </summary>
        private CommentLike()
        {
        }

        /// <summary>
        /// Composite PK'nin parçası — beğenilen yorumun Id'si. Create anında belirlenir ve değişmez.
        /// </summary>
        public Guid CommentId { get; private set; }

        /// <summary>Navigation property: Beğenilen yorum (N CommentLike - 1 Comment).</summary>
        public Comment Comment { get; private set; } = null!;

        /// <summary>
        /// Composite PK'nin parçası — beğeniyi yapan kullanıcının Id'si.
        /// IdentityUser'ın anahtar tipi string olduğu için string'dir.
        /// </summary>
        public string UserId { get; private set; } = null!;

        /// <summary>Navigation property: Beğeniyi yapan kullanıcı (N CommentLike - 1 User).</summary>
        public User User { get; private set; } = null!;

        /// <summary>Beğeninin oluşturulma anı (UTC).</summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Geçerli bir yorum beğenisi oluşturur. Yorum (<paramref name="commentId"/>) ve kullanıcı
        /// (<paramref name="userId"/>) zorunludur; <see cref="CreatedAt"/> oluşturma anındaki
        /// UTC zamana set edilir.
        /// </summary>
        public static CommentLike Create(Guid commentId, string userId)
        {
            if (commentId == Guid.Empty)
            {
                throw new ArgumentException("CommentLike must reference a valid comment.", nameof(commentId));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("CommentLike must reference a valid user.", nameof(userId));
            }

            return new CommentLike
            {
                CommentId = commentId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
