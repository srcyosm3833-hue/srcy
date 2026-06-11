using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// Uygulama kullanıcısı. IdentityUser'dan miras alır; IdentityUser'ın
    /// birincil anahtar tipi string'dir (Guid.NewGuid().ToString() ile üretilir).
    /// Id, UserName, Email, PasswordHash gibi alanlar Identity'den gelir.
    /// </summary>
    public class User : IdentityUser
    {
        /// <summary>Kullanıcının adı. Non-nullable.</summary>
        public string FirstName { get; set; } = null!;

        /// <summary>Kullanıcının soyadı. Non-nullable.</summary>
        public string LastName { get; set; } = null!;

        /// <summary>Profil fotoğrafı yolu/URL'i. Non-nullable.</summary>
        public string ImageUrl { get; set; } = null!;

        /// <summary>Navigation property: Kullanıcının yazdığı bloglar (1 User - N Blog).</summary>
        public ICollection<Blog> Blogs { get; set; } = new List<Blog>();

        /// <summary>Navigation property: Kullanıcının yaptığı yorumlar (1 User - N Comment).</summary>
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        /// <summary>Navigation property: Kullanıcının yaptığı alt yorumlar (1 User - N SubComment).</summary>
        public ICollection<SubComment> SubComments { get; set; } = new List<SubComment>();

        /// <summary>Navigation property: Kullanıcının refresh token'ları (1 User - N RefreshToken).</summary>
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
