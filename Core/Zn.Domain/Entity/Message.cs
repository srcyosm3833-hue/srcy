using System;
using Zn.Domain.Entity.Common;
using Zn.Domain.Exceptions;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// Ziyaretçiden gelen iletişim mesajını temsil eder. BaseEntity'den
    /// Guid tipinde Id, CreatedAt ve UpdatedAt alanlarını miras alır.
    /// <para>
    /// Invariant'lar (boş olmayan, azami uzunluğu aşmayan ad/e-posta/konu/gövde) factory metodu
    /// <see cref="Create"/> içinde korunur; geçersiz durumda <see cref="MessageDomainException"/>
    /// fırlatılır. Okunma durumu <see cref="MarkAsRead"/> mutator'ı ile açıkça (true/false) set
    /// edilir. Bu sayede geçersiz bir Message nesnesi hiçbir zaman var olamaz.
    /// </para>
    /// <para>
    /// Not: Property isimleri, tipleri ve kolon eşlemeleri bilinçli olarak değiştirilmemiştir;
    /// yalnızca davranış katmanı (factory + private set) eklenmiştir. Bu nedenle mevcut
    /// migration/şema ile birebir uyumludur — yeni migration gerekmez.
    /// </para>
    /// </summary>
    public class Message : BaseEntity, ISoftDeletable
    {
        /// <summary>
        /// Gönderen adının azami uzunluğu. MessageConfiguration'daki HasMaxLength(100) ile
        /// birebir senkrondur; veritabanı kısıtı ile domain invariant'ı paralel tutulur.
        /// </summary>
        public const int NameMaxLength = 100;

        /// <summary>Gönderen e-posta adresinin azami uzunluğu. MessageConfiguration'daki HasMaxLength(150) ile senkron.</summary>
        public const int EmailMaxLength = 150;

        /// <summary>Mesaj konusunun azami uzunluğu. MessageConfiguration'daki HasMaxLength(200) ile senkron.</summary>
        public const int SubjectMaxLength = 200;

        /// <summary>
        /// Mesaj gövdesinin azami uzunluğu. Kolon nvarchar(max) olduğundan veritabanı sınırı bu
        /// değerden geniştir; sabit, bot/spam yükünü kısmak için domain + validator seviyesinde
        /// uygulanan mantıksal üst sınırdır (kolon tipini DAR'altmaz, şema değişmez).
        /// </summary>
        public const int MessageBodyMaxLength = 2000;

        /// <summary>
        /// Hash'li IP alanının azami uzunluğu. Base64 kodlanmış SHA-256 çıktısı 44 karakterdir;
        /// 64'lük üst sınır olası kodlama değişikliklerine pay bırakır. MessageConfiguration
        /// HasMaxLength(64) ile senkron.
        /// </summary>
        public const int IpHashMaxLength = 64;

        /// <summary>
        /// EF Core materyalizasyonu için parametresiz constructor.
        /// Uygulama kodu yerine <see cref="Create"/> factory'sini kullanmalıdır.
        /// </summary>
        private Message()
        {
        }

        /// <summary>Gönderenin adı. Dışarıdan yalnızca okunabilir; <see cref="Create"/> ile belirlenir.</summary>
        public string Name { get; private set; } = null!;

        /// <summary>Gönderenin e-posta adresi. Dışarıdan yalnızca okunabilir; <see cref="Create"/> ile belirlenir.</summary>
        public string Email { get; private set; } = null!;

        /// <summary>Mesaj konusu. Dışarıdan yalnızca okunabilir; <see cref="Create"/> ile belirlenir.</summary>
        public string Subject { get; private set; } = null!;

        /// <summary>Mesaj içeriği. Dışarıdan yalnızca okunabilir; <see cref="Create"/> ile belirlenir.</summary>
        public string MessageBody { get; private set; } = null!;

        /// <summary>
        /// Mesajın okunup okunmadığı. Yeni mesaj okunmamış (false) başlar; değişiklik yalnızca
        /// <see cref="MarkAsRead"/> mutator'ı üzerinden yapılır.
        /// </summary>
        public bool IsRead { get; private set; }

        /// <summary>
        /// Kayıt soft delete edilmişse true. Dışarıdan yalnızca okunabilir; değişiklik
        /// <see cref="SoftDelete"/> mutator'ı üzerinden yapılır. EF Core global query filter'ı
        /// bu alana göre silinmiş mesajları varsayılan sorgulardan dışlar.
        /// </summary>
        public bool IsDeleted { get; private set; }

        /// <summary>Soft delete'in gerçekleştiği an (UTC); kayıt aktifse null.</summary>
        public DateTime? DeletedAt { get; private set; }

        /// <summary>
        /// Mesajı gönderen ziyaretçinin istemci IP adresinin tuzlu SHA-256 hash'i (anonim audit).
        /// Ham IP ASLA saklanmaz (KVKK). IP çözümlenemediğinde (örn. test ortamı) null kalır.
        /// Yalnızca admin mesaj kutusu görünümünde döner. Dışarıdan yalnızca okunabilir; yalnızca
        /// <see cref="Create"/> sırasında set edilir.
        /// </summary>
        public string? SenderIpHash { get; private set; }

        /// <summary>
        /// Ziyaretçi iletişim formundan geçerli bir Message oluşturur. Tüm metin alanları
        /// boş/whitespace olamaz ve ilgili azami uzunluğu aşamaz; aksi halde
        /// <see cref="MessageDomainException"/> fırlatılır. Mesaj okunmamış (IsRead=false) başlar.
        /// <paramref name="ipHash"/> opsiyoneldir (anonim audit): verilirse hash'li IP olarak
        /// saklanır, null/boş ise alan boş bırakılır.
        /// </summary>
        public static Message Create(string name, string email, string subject, string messageBody, string? ipHash = null)
        {
            return new Message
            {
                Id = Guid.NewGuid(),
                Name = Normalize(name, NameMaxLength, nameof(Name)),
                Email = Normalize(email, EmailMaxLength, nameof(Email)),
                Subject = Normalize(subject, SubjectMaxLength, nameof(Subject)),
                MessageBody = Normalize(messageBody, MessageBodyMaxLength, nameof(MessageBody)),
                SenderIpHash = NormalizeIpHash(ipHash),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Mesajın okunma durumunu açıkça (true/false) set eder ve
        /// <see cref="BaseEntity{TId}.UpdatedAt"/>'i günceller. Yönetici hem "okundu" hem
        /// "okunmadı" olarak işaretleyebildiği için explicit değer alır.
        /// </summary>
        public void MarkAsRead(bool isRead)
        {
            IsRead = isRead;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Mesajı soft delete olarak işaretler: <see cref="IsDeleted"/>=true ve
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

        /// <summary>
        /// Hash'li IP invariant'ı: null/boş ise null'a normalize edilir (audit opsiyoneldir),
        /// aksi halde azami uzunluk aşılırsa <see cref="MessageDomainException"/> fırlatılır.
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
                throw new MessageDomainException(
                    $"Sender IP hash cannot exceed {IpHashMaxLength} characters.");
            }

            return ipHash;
        }

        /// <summary>Metin alanı invariant'ı: boş olmama + trim + azami uzunluk.</summary>
        private static string Normalize(string value, int maxLength, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new MessageDomainException($"{fieldName} cannot be empty.");
            }

            string trimmed = value.Trim();

            if (trimmed.Length > maxLength)
            {
                throw new MessageDomainException(
                    $"{fieldName} cannot exceed {maxLength} characters.");
            }

            return trimmed;
        }
    }
}
