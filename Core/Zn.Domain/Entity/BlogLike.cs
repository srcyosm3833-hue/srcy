using System;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// Bir kullanıcının bir blogu beğendiğini temsil eden saf ilişki (join) tablosu.
    /// <para>
    /// Karar: Surrogate Id YOKTUR; birincil anahtar composite olarak
    /// (<see cref="BlogId"/>, <see cref="UserId"/>) çiftidir. Bu sayede aynı kullanıcının
    /// aynı blogu birden fazla kez beğenmesi veritabanı seviyesinde (PK constraint) engellenir
    /// ve beğeni doğal olarak idempotent olur. <see cref="Common.BaseEntity"/>'den miras ALMAZ —
    /// Id/equality davranışına ihtiyaç yoktur.
    /// </para>
    /// </summary>
    public class BlogLike
    {
        /// <summary>
        /// EF Core materyalizasyonu için parametresiz constructor.
        /// Uygulama kodu yerine <see cref="Create"/> factory'sini kullanmalıdır.
        /// </summary>
        private BlogLike()
        {
        }

        /// <summary>
        /// Composite PK'nin parçası — beğenilen blogun Id'si. Create anında belirlenir ve değişmez.
        /// </summary>
        public Guid BlogId { get; private set; }

        /// <summary>Navigation property: Beğenilen blog (N BlogLike - 1 Blog).</summary>
        public Blog Blog { get; private set; } = null!;

        /// <summary>
        /// Composite PK'nin parçası — beğeniyi yapan kullanıcının Id'si.
        /// IdentityUser'ın anahtar tipi string olduğu için string'dir.
        /// </summary>
        public string UserId { get; private set; } = null!;

        /// <summary>Navigation property: Beğeniyi yapan kullanıcı (N BlogLike - 1 User).</summary>
        public User User { get; private set; } = null!;

        /// <summary>Beğeninin oluşturulma anı (UTC).</summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Geçerli bir blog beğenisi oluşturur. Blog (<paramref name="blogId"/>) ve kullanıcı
        /// (<paramref name="userId"/>) zorunludur; <see cref="CreatedAt"/> oluşturma anındaki
        /// UTC zamana set edilir.
        /// </summary>
        public static BlogLike Create(Guid blogId, string userId)
        {
            if (blogId == Guid.Empty)
            {
                throw new ArgumentException("BlogLike must reference a valid blog.", nameof(blogId));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("BlogLike must reference a valid user.", nameof(userId));
            }

            return new BlogLike
            {
                BlogId = blogId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
