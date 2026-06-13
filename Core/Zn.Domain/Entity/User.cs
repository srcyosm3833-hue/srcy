using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Zn.Domain.Entity.Common;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// Uygulama kullanıcısı. IdentityUser'dan miras alır; IdentityUser'ın
    /// birincil anahtar tipi string'dir (Guid.NewGuid().ToString() ile üretilir).
    /// Id, UserName, Email, PasswordHash gibi alanlar Identity'den gelir.
    /// <para>
    /// Soft delete destekler (<see cref="ISoftDeletable"/>): kullanıcı kalıcı silinmek yerine
    /// işaretlenir. Kullanıcı silindiğinde blogları (A8 kararı) silinmez/anonimleştirilmez;
    /// yalnızca global query filter sayesinde kullanıcı kaydı listelenmez.
    /// </para>
    /// </summary>
    public class User : IdentityUser, ISoftDeletable
    {
        /// <summary>Kullanıcının adı. Non-nullable.</summary>
        public string FirstName { get; set; } = null!;

        /// <summary>Kullanıcının soyadı. Non-nullable.</summary>
        public string LastName { get; set; } = null!;

        /// <summary>Profil fotoğrafı yolu/URL'i. Non-nullable.</summary>
        public string ImageUrl { get; set; } = null!;

        /// <summary>
        /// Hesabın oluşturulma anı (UTC). IdentityUser'da yerleşik bir oluşturulma damgası
        /// bulunmadığından bu alan eklenmiştir; admin kullanıcı listesinde kayıt tarihini
        /// göstermek için kullanılır. Varsayılan değer kayıt anındaki UTC zamandır.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Kullanıcı soft delete edilmişse true. Dışarıdan yalnızca okunabilir; değişiklik
        /// <see cref="SoftDelete"/> mutator'ı üzerinden yapılır. EF Core global query filter'ı
        /// bu alana göre silinmiş kullanıcıları varsayılan sorgulardan dışlar.
        /// </summary>
        public bool IsDeleted { get; private set; }

        /// <summary>Soft delete'in gerçekleştiği an (UTC); kullanıcı aktifse null.</summary>
        public DateTime? DeletedAt { get; private set; }

        /// <summary>Navigation property: Kullanıcının yazdığı bloglar (1 User - N Blog).</summary>
        public ICollection<Blog> Blogs { get; set; } = new List<Blog>();

        /// <summary>Navigation property: Kullanıcının yaptığı yorumlar (1 User - N Comment).</summary>
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        /// <summary>Navigation property: Kullanıcının yaptığı alt yorumlar (1 User - N SubComment).</summary>
        public ICollection<SubComment> SubComments { get; set; } = new List<SubComment>();

        /// <summary>Navigation property: Kullanıcının refresh token'ları (1 User - N RefreshToken).</summary>
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        /// <summary>
        /// Kullanıcıyı soft delete olarak işaretler: <see cref="IsDeleted"/>=true ve
        /// <see cref="DeletedAt"/>=DateTime.UtcNow set eder. Kullanıcı kaydı veritabanında kalır;
        /// global query filter sayesinde sonraki sorgularda görünmez. Blogları (A8) etkilenmez.
        /// Zaten silinmiş bir kullanıcıda çağrılması idempotenttir (durum değişmez).
        /// </summary>
        public void SoftDelete()
        {
            if (IsDeleted)
            {
                return;
            }

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }
    }
}
