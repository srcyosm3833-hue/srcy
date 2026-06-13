using System;
using System.Collections.Generic;
using Zn.Domain.Entity.Common;
using Zn.Domain.Exceptions;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// Blog yazısını temsil eder. BaseEntity'den Guid tipinde Id,
    /// CreatedAt ve UpdatedAt alanlarını miras alır.
    /// <para>
    /// Invariant'lar (boş olmayan başlık/açıklama/görseller, azami uzunlukları aşmama,
    /// geçerli kategori ve yazar referansı) factory metodu <see cref="Create"/> ve mutator
    /// <see cref="Update"/> içinde korunur; geçersiz durumda <see cref="BlogDomainException"/>
    /// fırlatılır. Bu sayede geçersiz bir Blog nesnesi hiçbir zaman var olamaz.
    /// </para>
    /// <para>
    /// Not: Property isimleri, tipleri ve kolon eşlemeleri bilinçli olarak değiştirilmemiştir;
    /// yalnızca davranış katmanı (factory + private set) eklenmiştir. Bu nedenle mevcut
    /// migration/şema ile birebir uyumludur — yeni migration gerekmez.
    /// </para>
    /// </summary>
    public class Blog : BaseEntity, ISoftDeletable
    {
        /// <summary>Başlığın azami uzunluğu. BlogConfiguration'daki HasMaxLength(150) ile senkron.</summary>
        public const int TitleMaxLength = 150;

        /// <summary>Görsel yolu/URL alanlarının azami uzunluğu. BlogConfiguration HasMaxLength(500) ile senkron.</summary>
        public const int ImageUrlMaxLength = 500;

        /// <summary>
        /// Hash'li IP alanlarının azami uzunluğu. Base64 kodlanmış SHA-256 çıktısı 44 karakterdir;
        /// 64'lük üst sınır olası kodlama değişikliklerine pay bırakır. BlogConfiguration
        /// HasMaxLength(64) ile senkron.
        /// </summary>
        public const int IpHashMaxLength = 64;

        /// <summary>
        /// EF Core materyalizasyonu için parametresiz constructor.
        /// Uygulama kodu yerine <see cref="Create"/> factory'sini kullanmalıdır.
        /// </summary>
        private Blog()
        {
        }

        /// <summary>Blog başlığı. Dışarıdan yalnızca okunabilir; değişiklik <see cref="Update"/> üzerinden yapılır.</summary>
        public string Title { get; private set; } = null!;

        /// <summary>Kapak görseli yolu/URL'i. Dışarıdan yalnızca okunabilir.</summary>
        public string CoverImage { get; private set; } = null!;

        /// <summary>Blog içerik görseli yolu/URL'i. Dışarıdan yalnızca okunabilir.</summary>
        public string BlogImage { get; private set; } = null!;

        /// <summary>Blog açıklaması/içeriği. Dışarıdan yalnızca okunabilir.</summary>
        public string Description { get; private set; } = null!;

        /// <summary>
        /// Foreign key: Bağlı olduğu kategorinin Id'si.
        /// Guid value-type olduğu için zaten non-nullable'dır; her blog bir kategoriye ait olmak zorundadır.
        /// </summary>
        public Guid CategoryId { get; private set; }

        /// <summary>
        /// Navigation property: Blogun ait olduğu kategori (N Blog - 1 Kategori).
        /// Non-nullable; ilişki zorunluluğu Fluent API tarafında IsRequired ile tanımlanır.
        /// </summary>
        public Category Category { get; private set; } = null!;

        /// <summary>
        /// Foreign key: Blogu yazan kullanıcının Id'si.
        /// IdentityUser'ın anahtar tipi string olduğu için string'dir. Yazar create anında belirlenir
        /// ve değiştirilemez (sahiplik devri iş kuralı olarak desteklenmez).
        /// </summary>
        public string UserId { get; private set; } = null!;

        /// <summary>Navigation property: Blogu yazan kullanıcı (N Blog - 1 User).</summary>
        public User User { get; private set; } = null!;

        /// <summary>Navigation property: Bloga yapılan yorumlar (1 Blog - N Comment).</summary>
        public ICollection<Comment> Comments { get; private set; } = new List<Comment>();

        /// <summary>
        /// Navigation property: Bloga yapılan beğeniler (1 Blog - N BlogLike). Beğeni sayısı
        /// ve "mevcut kullanıcı beğendi mi" bilgisi okuma projeksiyonlarında bu koleksiyon
        /// üzerinden DB seviyesinde (COUNT / EXISTS) hesaplanır; koleksiyon belleğe çekilmez.
        /// </summary>
        public ICollection<BlogLike> Likes { get; private set; } = new List<BlogLike>();

        /// <summary>
        /// Kayıt soft delete edilmişse true. Dışarıdan yalnızca okunabilir; değişiklik
        /// <see cref="SoftDelete"/> mutator'ı üzerinden yapılır. EF Core global query filter'ı
        /// bu alana göre silinmiş blogları varsayılan sorgulardan dışlar.
        /// </summary>
        public bool IsDeleted { get; private set; }

        /// <summary>Soft delete'in gerçekleştiği an (UTC); kayıt aktifse null.</summary>
        public DateTime? DeletedAt { get; private set; }

        /// <summary>
        /// Blogu oluşturan kullanıcının istemci IP adresinin tuzlu SHA-256 hash'i (anonim audit).
        /// Ham IP ASLA saklanmaz (KVKK). IP çözümlenemediğinde (örn. test ortamı) null kalır.
        /// Yalnızca admin audit görünümünde döner; public yanıtlarda hiçbir zaman yer almaz.
        /// Dışarıdan yalnızca okunabilir; yalnızca <see cref="Create"/> sırasında set edilir.
        /// </summary>
        public string? CreatorIpHash { get; private set; }

        /// <summary>
        /// Geçerli bir Blog oluşturur. Tüm zorunlu alanlar boş/whitespace olamaz ve azami
        /// uzunluklarını aşamaz; aksi halde <see cref="BlogDomainException"/> fırlatılır.
        /// Yazar (<paramref name="userId"/>) ve kategori (<paramref name="categoryId"/>)
        /// zorunludur. <paramref name="ipHash"/> opsiyoneldir (anonim audit): verilirse hash'li
        /// IP olarak saklanır, null/boş ise alan boş bırakılır.
        /// </summary>
        public static Blog Create(
            string title,
            string description,
            string coverImage,
            string blogImage,
            Guid categoryId,
            string userId,
            string? ipHash = null)
        {
            if (categoryId == Guid.Empty)
            {
                throw new BlogDomainException("Blog must belong to a valid category.");
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new BlogDomainException("Blog must have an author.");
            }

            return new Blog
            {
                Id = Guid.NewGuid(),
                Title = NormalizeText(title, nameof(title), TitleMaxLength),
                Description = NormalizeDescription(description),
                CoverImage = NormalizeText(coverImage, nameof(coverImage), ImageUrlMaxLength),
                BlogImage = NormalizeText(blogImage, nameof(blogImage), ImageUrlMaxLength),
                CategoryId = categoryId,
                UserId = userId,
                CreatorIpHash = NormalizeIpHash(ipHash),
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Blogun içeriğini ve kategorisini invariant kontrolüyle günceller ve
        /// <see cref="BaseEntity{TId}.UpdatedAt"/>'i günceller. Yazar (UserId) değişmez.
        /// Geçersiz değerler <see cref="BlogDomainException"/> fırlatır.
        /// </summary>
        public void Update(
            string title,
            string description,
            string coverImage,
            string blogImage,
            Guid categoryId)
        {
            if (categoryId == Guid.Empty)
            {
                throw new BlogDomainException("Blog must belong to a valid category.");
            }

            Title = NormalizeText(title, nameof(title), TitleMaxLength);
            Description = NormalizeDescription(description);
            CoverImage = NormalizeText(coverImage, nameof(coverImage), ImageUrlMaxLength);
            BlogImage = NormalizeText(blogImage, nameof(blogImage), ImageUrlMaxLength);
            CategoryId = categoryId;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Blogu soft delete olarak işaretler: <see cref="IsDeleted"/>=true ve
        /// <see cref="DeletedAt"/>=DateTime.UtcNow set eder, <see cref="BaseEntity{TId}.UpdatedAt"/>'i
        /// günceller. Kayıt veritabanında kalır; global query filter sayesinde sonraki sorgularda
        /// görünmez. Zaten silinmiş bir kayıtta çağrılması idempotenttir (durum değişmez).
        /// </summary>
        public void SoftDelete()
        {
            if (IsDeleted)
            {
                return;
            }

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>Tek satırlık metin invariant'ı: boş olmama + trim + azami uzunluk.</summary>
        private static string NormalizeText(string value, string fieldName, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new BlogDomainException($"Blog {fieldName} cannot be empty.");
            }

            string trimmed = value.Trim();

            if (trimmed.Length > maxLength)
            {
                throw new BlogDomainException(
                    $"Blog {fieldName} cannot exceed {maxLength} characters.");
            }

            return trimmed;
        }

        /// <summary>
        /// Hash'li IP invariant'ı: null/boş ise null'a normalize edilir (audit opsiyoneldir),
        /// aksi halde azami uzunluk aşılırsa <see cref="BlogDomainException"/> fırlatılır.
        /// Hash deterministik base64 olduğundan trim edilmez.
        /// </summary>
        private static string? NormalizeIpHash(string? ipHash)
        {
            if (string.IsNullOrWhiteSpace(ipHash))
            {
                return null;
            }

            if (ipHash.Length > IpHashMaxLength)
            {
                throw new BlogDomainException(
                    $"Blog creator IP hash cannot exceed {IpHashMaxLength} characters.");
            }

            return ipHash;
        }

        /// <summary>Açıklama invariant'ı: boş olmama (uzunluk sınırı yok — nvarchar(max)).</summary>
        private static string NormalizeDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new BlogDomainException("Blog description cannot be empty.");
            }

            return description.Trim();
        }
    }
}
