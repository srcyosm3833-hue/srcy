using System;
using Zn.Domain.Entity.Common;
using Zn.Domain.Exceptions;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// İletişim bilgilerini temsil eder. BaseEntity'den Guid tipinde Id,
    /// CreatedAt ve UpdatedAt alanlarını miras alır.
    /// <para>
    /// Invariant'lar (boş olmayan, azami uzunluğu aşmayan adres/e-posta/telefon/harita URL'i)
    /// factory metodu <see cref="Create"/> ve mutator <see cref="Update"/> içinde korunur;
    /// geçersiz durumda <see cref="ContactDomainException"/> fırlatılır. Uygulama tek bir Contact
    /// kaydı tutar (upsert ile yönetilir); bu sayede geçersiz bir Contact nesnesi hiçbir zaman var olamaz.
    /// </para>
    /// <para>
    /// Not: Property isimleri, tipleri ve kolon eşlemeleri bilinçli olarak değiştirilmemiştir;
    /// yalnızca davranış katmanı (factory + private set) eklenmiştir. Bu nedenle mevcut
    /// migration/şema ile birebir uyumludur — yeni migration gerekmez.
    /// </para>
    /// </summary>
    public class Contact : BaseEntity
    {
        /// <summary>Adresin azami uzunluğu. ContactConfiguration'daki HasMaxLength(300) ile senkron.</summary>
        public const int AddressMaxLength = 300;

        /// <summary>E-posta adresinin azami uzunluğu. ContactConfiguration'daki HasMaxLength(150) ile senkron.</summary>
        public const int EmailMaxLength = 150;

        /// <summary>Telefon numarasının azami uzunluğu. ContactConfiguration'daki HasMaxLength(20) ile senkron.</summary>
        public const int PhoneMaxLength = 20;

        /// <summary>Harita URL'inin azami uzunluğu. ContactConfiguration'daki HasMaxLength(1000) ile senkron.</summary>
        public const int MapUrlMaxLength = 1000;

        /// <summary>
        /// EF Core materyalizasyonu için parametresiz constructor.
        /// Uygulama kodu yerine <see cref="Create"/> factory'sini kullanmalıdır.
        /// </summary>
        private Contact()
        {
        }

        /// <summary>Açık adres. Dışarıdan yalnızca okunabilir; değişiklik <see cref="Update"/> üzerinden yapılır.</summary>
        public string Address { get; private set; } = null!;

        /// <summary>E-posta adresi. Dışarıdan yalnızca okunabilir; değişiklik <see cref="Update"/> üzerinden yapılır.</summary>
        public string Email { get; private set; } = null!;

        /// <summary>Telefon numarası. Dışarıdan yalnızca okunabilir; değişiklik <see cref="Update"/> üzerinden yapılır.</summary>
        public string Phone { get; private set; } = null!;

        /// <summary>Harita (embed/konum) URL'i. Dışarıdan yalnızca okunabilir; değişiklik <see cref="Update"/> üzerinden yapılır.</summary>
        public string MapUrl { get; private set; } = null!;

        /// <summary>
        /// Geçerli bir Contact oluşturur. Tüm alanlar boş/whitespace olamaz ve ilgili azami
        /// uzunluğu aşamaz; aksi halde <see cref="ContactDomainException"/> fırlatılır.
        /// </summary>
        public static Contact Create(string address, string email, string phone, string mapUrl)
        {
            return new Contact
            {
                Id = Guid.NewGuid(),
                Address = Normalize(address, AddressMaxLength, nameof(Address)),
                Email = Normalize(email, EmailMaxLength, nameof(Email)),
                Phone = Normalize(phone, PhoneMaxLength, nameof(Phone)),
                MapUrl = Normalize(mapUrl, MapUrlMaxLength, nameof(MapUrl)),
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// İletişim bilgilerini invariant kontrolüyle günceller ve
        /// <see cref="BaseEntity{TId}.UpdatedAt"/>'i set eder. Geçersiz değer
        /// <see cref="ContactDomainException"/> fırlatır.
        /// </summary>
        public void Update(string address, string email, string phone, string mapUrl)
        {
            Address = Normalize(address, AddressMaxLength, nameof(Address));
            Email = Normalize(email, EmailMaxLength, nameof(Email));
            Phone = Normalize(phone, PhoneMaxLength, nameof(Phone));
            MapUrl = Normalize(mapUrl, MapUrlMaxLength, nameof(MapUrl));
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>Metin alanı invariant'ı: boş olmama + trim + azami uzunluk.</summary>
        private static string Normalize(string value, int maxLength, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ContactDomainException($"{fieldName} cannot be empty.");
            }

            string trimmed = value.Trim();

            if (trimmed.Length > maxLength)
            {
                throw new ContactDomainException(
                    $"{fieldName} cannot exceed {maxLength} characters.");
            }

            return trimmed;
        }
    }
}
