using System;
using System.Collections.Generic;
using Zn.Domain.Entity.Common;
using Zn.Domain.Exceptions;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// Blog yazısına yapılan yorumu temsil eder. BaseEntity'den
    /// Guid tipinde Id, CreatedAt ve UpdatedAt alanlarını miras alır.
    /// <para>
    /// Invariant'lar (boş olmayan yorum metni, azami uzunluğu aşmama, geçerli blog ve yazar
    /// referansı) factory metodu <see cref="Create"/> ve mutator <see cref="Update"/> içinde
    /// korunur; geçersiz durumda <see cref="CommentDomainException"/> fırlatılır. Bu sayede
    /// geçersiz bir Comment nesnesi hiçbir zaman var olamaz.
    /// </para>
    /// <para>
    /// Not: Property isimleri, tipleri ve kolon eşlemeleri bilinçli olarak değiştirilmemiştir;
    /// yalnızca davranış katmanı (factory + private set) eklenmiştir. Bu nedenle mevcut
    /// migration/şema ile birebir uyumludur — yeni migration gerekmez.
    /// </para>
    /// </summary>
    public class Comment : BaseEntity
    {
        /// <summary>Yorum metninin azami uzunluğu. CommentConfiguration'daki HasMaxLength(1000) ile senkron.</summary>
        public const int CommentTextMaxLength = 1000;

        /// <summary>
        /// EF Core materyalizasyonu için parametresiz constructor.
        /// Uygulama kodu yerine <see cref="Create"/> factory'sini kullanmalıdır.
        /// </summary>
        private Comment()
        {
        }

        /// <summary>Yorum içeriği. Dışarıdan yalnızca okunabilir; değişiklik <see cref="Update"/> üzerinden yapılır.</summary>
        public string CommentText { get; private set; } = null!;

        /// <summary>Foreign key: Yorumun yapıldığı blogun Id'si. Create anında belirlenir ve değişmez.</summary>
        public Guid BlogId { get; private set; }

        /// <summary>Navigation property: Yorumun yapıldığı blog (N Comment - 1 Blog).</summary>
        public Blog Blog { get; private set; } = null!;

        /// <summary>
        /// Foreign key: Yorumu yapan kullanıcının Id'si.
        /// IdentityUser'ın anahtar tipi string olduğu için string'dir. Create anında belirlenir ve değişmez.
        /// </summary>
        public string UserId { get; private set; } = null!;

        /// <summary>Navigation property: Yorumu yapan kullanıcı (N Comment - 1 User).</summary>
        public User User { get; private set; } = null!;

        /// <summary>Navigation property: Yoruma verilen yanıtlar (1 Comment - N SubComment).</summary>
        public ICollection<SubComment> SubComments { get; private set; } = new List<SubComment>();

        /// <summary>
        /// Navigation property: Yoruma yapılan beğeniler (1 Comment - N CommentLike). Beğeni sayısı
        /// ve "mevcut kullanıcı beğendi mi" bilgisi okuma projeksiyonlarında bu koleksiyon
        /// üzerinden DB seviyesinde (COUNT / EXISTS) hesaplanır; koleksiyon belleğe çekilmez.
        /// </summary>
        public ICollection<CommentLike> Likes { get; private set; } = new List<CommentLike>();

        /// <summary>
        /// Geçerli bir Comment oluşturur. Yorum metni boş/whitespace olamaz ve azami uzunluğunu
        /// aşamaz; aksi halde <see cref="CommentDomainException"/> fırlatılır. Blog
        /// (<paramref name="blogId"/>) ve yazar (<paramref name="userId"/>) zorunludur.
        /// </summary>
        public static Comment Create(Guid blogId, string userId, string commentText)
        {
            if (blogId == Guid.Empty)
            {
                throw new CommentDomainException("Comment must belong to a valid blog.");
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new CommentDomainException("Comment must have an author.");
            }

            return new Comment
            {
                Id = Guid.NewGuid(),
                BlogId = blogId,
                UserId = userId,
                CommentText = NormalizeText(commentText),
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Yorum metnini invariant kontrolüyle günceller ve
        /// <see cref="BaseEntity{TId}.UpdatedAt"/>'i set eder. Blog ve yazar (UserId) değişmez.
        /// Geçersiz değer <see cref="CommentDomainException"/> fırlatır.
        /// </summary>
        public void Update(string commentText)
        {
            CommentText = NormalizeText(commentText);
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>Yorum metni invariant'ı: boş olmama + trim + azami uzunluk.</summary>
        private static string NormalizeText(string commentText)
        {
            if (string.IsNullOrWhiteSpace(commentText))
            {
                throw new CommentDomainException("Comment text cannot be empty.");
            }

            string trimmed = commentText.Trim();

            if (trimmed.Length > CommentTextMaxLength)
            {
                throw new CommentDomainException(
                    $"Comment text cannot exceed {CommentTextMaxLength} characters.");
            }

            return trimmed;
        }
    }
}
