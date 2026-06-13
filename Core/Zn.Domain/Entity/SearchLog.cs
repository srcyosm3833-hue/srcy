using System;
using Zn.Domain.Exceptions;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// Blog araması yapıldığında kaydedilen audit log kaydını temsil eder. Bilinçli olarak
    /// <see cref="Common.BaseEntity"/>'den TÜREMEZ: bir log kaydı asla güncellenmez (UpdatedAt
    /// gerekmez), yalnızca yazılır ve listelenir. Bu yüzden yalın bir entity'dir.
    /// <para>
    /// Invariant'lar (boş olmayan terim, azami uzunlukları aşmama) factory metodu
    /// <see cref="Create"/> içinde korunur; geçersiz durumda <see cref="SearchLogDomainException"/>
    /// fırlatılır. "Kim aradı" sorusu hem <see cref="UserId"/> (giriş yapılmışsa) hem de
    /// <see cref="UserFullName"/> (log anındaki ad-soyad snapshot'ı) ile yanıtlanır; böylece
    /// kullanıcı sonradan silinse/yeniden adlandırılsa bile log anındaki kimlik korunur.
    /// </para>
    /// <para>
    /// IP daima tuzlu SHA-256 hash olarak (<see cref="IpHash"/>) saklanır; ham IP ASLA tutulmaz
    /// (KVKK). Anonim aramada <see cref="UserId"/> ve <see cref="UserFullName"/> null olur.
    /// </para>
    /// </summary>
    public class SearchLog
    {
        /// <summary>Aranan terimin azami uzunluğu. SearchLogConfiguration HasMaxLength(200) ile senkron.</summary>
        public const int TermMaxLength = 200;

        /// <summary>Kullanıcı tam adı snapshot'ının azami uzunluğu. SearchLogConfiguration HasMaxLength(256) ile senkron.</summary>
        public const int UserFullNameMaxLength = 256;

        /// <summary>Hash'li IP alanının azami uzunluğu. SearchLogConfiguration HasMaxLength(64) ile senkron.</summary>
        public const int IpHashMaxLength = 64;

        /// <summary>
        /// EF Core materyalizasyonu için parametresiz constructor.
        /// Uygulama kodu yerine <see cref="Create"/> factory'sini kullanmalıdır.
        /// </summary>
        private SearchLog()
        {
        }

        /// <summary>Log kaydının benzersiz kimliği.</summary>
        public Guid Id { get; private set; }

        /// <summary>Aranan terim. Boş/whitespace olamaz; azami uzunluğu aşamaz.</summary>
        public string Term { get; private set; } = null!;

        /// <summary>
        /// Aramayı yapan kullanıcının kimliği (AspNetUsers FK). Anonim aramada null.
        /// IdentityUser anahtar tipi string olduğundan string'dir.
        /// </summary>
        public string? UserId { get; private set; }

        /// <summary>
        /// Log anındaki kullanıcı tam adı snapshot'ı (FirstName + " " + LastName). Anonimde null.
        /// Kullanıcı sonradan silinse/yeniden adlandırılsa bile log anındaki kimlik korunur.
        /// </summary>
        public string? UserFullName { get; private set; }

        /// <summary>
        /// Aramayı yapan istemcinin IP adresinin tuzlu SHA-256 hash'i (anonim audit). Ham IP ASLA
        /// saklanmaz (KVKK). IP çözümlenemediğinde null kalır.
        /// </summary>
        public string? IpHash { get; private set; }

        /// <summary>Aramanın gerçekleştiği an (UTC). İndekslidir (tarih azalan listeleme için).</summary>
        public DateTime SearchedAt { get; private set; }

        /// <summary>
        /// Geçerli bir arama log kaydı oluşturur. <paramref name="term"/> boş/whitespace olamaz ve
        /// azami uzunluğu aşamaz; <paramref name="userFullName"/> ve <paramref name="ipHash"/>
        /// verilirse azami uzunlukları aşamaz; aksi halde <see cref="SearchLogDomainException"/>
        /// fırlatılır. <paramref name="userId"/> ve <paramref name="userFullName"/> anonim aramada
        /// null geçilebilir.
        /// </summary>
        public static SearchLog Create(
            string term,
            string? userId,
            string? userFullName,
            string? ipHash)
        {
            return new SearchLog
            {
                Id = Guid.NewGuid(),
                Term = NormalizeTerm(term),
                UserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
                UserFullName = NormalizeOptional(userFullName, UserFullNameMaxLength, nameof(UserFullName)),
                IpHash = NormalizeOptional(ipHash, IpHashMaxLength, nameof(IpHash)),
                SearchedAt = DateTime.UtcNow
            };
        }

        /// <summary>Terim invariant'ı: boş olmama + trim + azami uzunluk.</summary>
        private static string NormalizeTerm(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                throw new SearchLogDomainException("Search term cannot be empty.");
            }

            string trimmed = term.Trim();

            if (trimmed.Length > TermMaxLength)
            {
                throw new SearchLogDomainException(
                    $"Search term cannot exceed {TermMaxLength} characters.");
            }

            return trimmed;
        }

        /// <summary>
        /// Opsiyonel metin alanı invariant'ı: null/boş ise null'a normalize edilir, aksi halde
        /// azami uzunluk aşılırsa <see cref="SearchLogDomainException"/> fırlatılır. IP hash gibi
        /// deterministik değerleri bozmamak için trim edilmez.
        /// </summary>
        private static string? NormalizeOptional(string? value, int maxLength, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (value.Length > maxLength)
            {
                throw new SearchLogDomainException(
                    $"{fieldName} cannot exceed {maxLength} characters.");
            }

            return value;
        }
    }
}
