using System;
using Zn.Domain.Entity.Common;
using Zn.Domain.Exceptions;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// Bir yoruma verilen yanıtı (alt yorum) temsil eder. BaseEntity'den
    /// Guid tipinde Id, CreatedAt ve UpdatedAt alanlarını miras alır.
    /// <para>
    /// Invariant'lar (boş olmayan alt yorum metni, azami uzunluğu aşmama, geçerli yorum ve yazar
    /// referansı) factory metodu <see cref="Create"/> ve mutator <see cref="Update"/> içinde
    /// korunur; geçersiz durumda <see cref="SubCommentDomainException"/> fırlatılır. Bu sayede
    /// geçersiz bir SubComment nesnesi hiçbir zaman var olamaz.
    /// </para>
    /// <para>
    /// Not: Property isimleri, tipleri ve kolon eşlemeleri bilinçli olarak değiştirilmemiştir;
    /// yalnızca davranış katmanı (factory + private set) eklenmiştir. Bu nedenle mevcut
    /// migration/şema ile birebir uyumludur — yeni migration gerekmez.
    /// </para>
    /// </summary>
    public class SubComment : BaseEntity
    {
        /// <summary>Alt yorum metninin azami uzunluğu. SubCommentConfiguration'daki HasMaxLength(1000) ile senkron.</summary>
        public const int SubCommentTextMaxLength = 1000;

        /// <summary>
        /// EF Core materyalizasyonu için parametresiz constructor.
        /// Uygulama kodu yerine <see cref="Create"/> factory'sini kullanmalıdır.
        /// </summary>
        private SubComment()
        {
        }

        /// <summary>Alt yorum içeriği. Dışarıdan yalnızca okunabilir; değişiklik <see cref="Update"/> üzerinden yapılır.</summary>
        public string SubCommentText { get; private set; } = null!;

        /// <summary>Foreign key: Yanıtlanan ana yorumun Id'si. Create anında belirlenir ve değişmez.</summary>
        public Guid CommentId { get; private set; }

        /// <summary>Navigation property: Yanıtlanan ana yorum (N SubComment - 1 Comment).</summary>
        public Comment Comment { get; private set; } = null!;

        /// <summary>
        /// Foreign key: Alt yorumu yapan kullanıcının Id'si.
        /// IdentityUser'ın anahtar tipi string olduğu için string'dir. Create anında belirlenir ve değişmez.
        /// </summary>
        public string UserId { get; private set; } = null!;

        /// <summary>Navigation property: Alt yorumu yapan kullanıcı (N SubComment - 1 User).</summary>
        public User User { get; private set; } = null!;

        /// <summary>
        /// Geçerli bir SubComment oluşturur. Alt yorum metni boş/whitespace olamaz ve azami
        /// uzunluğunu aşamaz; aksi halde <see cref="SubCommentDomainException"/> fırlatılır. Ana
        /// yorum (<paramref name="commentId"/>) ve yazar (<paramref name="userId"/>) zorunludur.
        /// </summary>
        public static SubComment Create(Guid commentId, string userId, string subCommentText)
        {
            if (commentId == Guid.Empty)
            {
                throw new SubCommentDomainException("Sub-comment must belong to a valid comment.");
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new SubCommentDomainException("Sub-comment must have an author.");
            }

            return new SubComment
            {
                Id = Guid.NewGuid(),
                CommentId = commentId,
                UserId = userId,
                SubCommentText = NormalizeText(subCommentText),
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Alt yorum metnini invariant kontrolüyle günceller ve
        /// <see cref="BaseEntity{TId}.UpdatedAt"/>'i set eder. Ana yorum ve yazar (UserId) değişmez.
        /// Geçersiz değer <see cref="SubCommentDomainException"/> fırlatır.
        /// </summary>
        public void Update(string subCommentText)
        {
            SubCommentText = NormalizeText(subCommentText);
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>Alt yorum metni invariant'ı: boş olmama + trim + azami uzunluk.</summary>
        private static string NormalizeText(string subCommentText)
        {
            if (string.IsNullOrWhiteSpace(subCommentText))
            {
                throw new SubCommentDomainException("Sub-comment text cannot be empty.");
            }

            string trimmed = subCommentText.Trim();

            if (trimmed.Length > SubCommentTextMaxLength)
            {
                throw new SubCommentDomainException(
                    $"Sub-comment text cannot exceed {SubCommentTextMaxLength} characters.");
            }

            return trimmed;
        }
    }
}
