using System;
using Zn.Domain.Entity.Common;
using Zn.Domain.Exceptions;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// Sosyal medya hesabı bağlantısını temsil eder. BaseEntity'den
    /// Guid tipinde Id, CreatedAt ve UpdatedAt alanlarını miras alır.
    /// <para>
    /// Invariant'lar (boş olmayan, azami uzunluğu aşmayan Title/Url/Icon) factory metodu
    /// <see cref="Create"/> ve mutator <see cref="Update"/> içinde korunur; geçersiz
    /// durumda <see cref="SocialMediaDomainException"/> fırlatılır. Bu sayede geçersiz bir
    /// SocialMedia nesnesi hiçbir zaman var olamaz. Şema değişmez: alanların isim/tipleri
    /// önceki anemik sürümle birebir aynıdır (SocialMediaConfiguration ile senkron).
    /// </para>
    /// </summary>
    public class SocialMedia : BaseEntity
    {
        /// <summary>
        /// Platform adının azami uzunluğu. SocialMediaConfiguration'daki
        /// HasMaxLength(50) ile birebir aynıdır; DB kısıtı ile domain invariant'ı senkron tutulur.
        /// </summary>
        public const int TitleMaxLength = 50;

        /// <summary>
        /// Bağlantı (URL) alanının azami uzunluğu. SocialMediaConfiguration'daki
        /// HasMaxLength(500) ile birebir aynıdır.
        /// </summary>
        public const int UrlMaxLength = 500;

        /// <summary>
        /// İkon alanının azami uzunluğu. SocialMediaConfiguration'daki
        /// HasMaxLength(100) ile birebir aynıdır.
        /// </summary>
        public const int IconMaxLength = 100;

        /// <summary>
        /// EF Core materyalizasyonu için parametresiz constructor.
        /// Uygulama kodu yerine <see cref="Create"/> factory'sini kullanmalıdır.
        /// </summary>
        private SocialMedia()
        {
        }

        /// <summary>
        /// Platform adı (Instagram, X, LinkedIn vb.). Dışarıdan yalnızca okunabilir;
        /// değişiklik <see cref="Update"/> üzerinden invariant kontrolüyle yapılır.
        /// </summary>
        public string Title { get; private set; } = null!;

        /// <summary>Profil/hesap bağlantısı. Yalnızca okunabilir; değişiklik <see cref="Update"/> ile.</summary>
        public string Url { get; private set; } = null!;

        /// <summary>İkon CSS sınıfı veya ikon dosya yolu. Yalnızca okunabilir; değişiklik <see cref="Update"/> ile.</summary>
        public string Icon { get; private set; } = null!;

        /// <summary>
        /// Geçerli bir SocialMedia oluşturur. Title/Url/Icon boş/whitespace olamaz ve
        /// ilgili azami uzunlukları aşamaz; aksi halde <see cref="SocialMediaDomainException"/> fırlatılır.
        /// </summary>
        /// <param name="title">Platform adı (trim edilerek saklanır).</param>
        /// <param name="url">Profil/hesap bağlantısı (trim edilerek saklanır).</param>
        /// <param name="icon">İkon CSS sınıfı veya yolu (trim edilerek saklanır).</param>
        public static SocialMedia Create(string title, string url, string icon)
        {
            return new SocialMedia
            {
                Id = Guid.NewGuid(),
                Title = NormalizeTitle(title),
                Url = NormalizeUrl(url),
                Icon = NormalizeIcon(icon),
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Title/Url/Icon alanlarını invariant kontrolüyle değiştirir ve
        /// <see cref="BaseEntity{TId}.UpdatedAt"/>'i günceller. Geçersiz değer
        /// <see cref="SocialMediaDomainException"/> fırlatır.
        /// </summary>
        public void Update(string title, string url, string icon)
        {
            Title = NormalizeTitle(title);
            Url = NormalizeUrl(url);
            Icon = NormalizeIcon(icon);
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>Title invariant'larını uygular: trim + boşluk + uzunluk kontrolü.</summary>
        private static string NormalizeTitle(string title) =>
            Normalize(title, TitleMaxLength, nameof(Title));

        /// <summary>Url invariant'larını uygular: trim + boşluk + uzunluk kontrolü.</summary>
        private static string NormalizeUrl(string url) =>
            Normalize(url, UrlMaxLength, nameof(Url));

        /// <summary>Icon invariant'larını uygular: trim + boşluk + uzunluk kontrolü.</summary>
        private static string NormalizeIcon(string icon) =>
            Normalize(icon, IconMaxLength, nameof(Icon));

        /// <summary>Ortak string invariant'ı: trim + boş kontrolü + azami uzunluk.</summary>
        private static string Normalize(string value, int maxLength, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new SocialMediaDomainException($"{fieldName} cannot be empty.");
            }

            string trimmed = value.Trim();

            if (trimmed.Length > maxLength)
            {
                throw new SocialMediaDomainException(
                    $"{fieldName} cannot exceed {maxLength} characters.");
            }

            return trimmed;
        }
    }
}
