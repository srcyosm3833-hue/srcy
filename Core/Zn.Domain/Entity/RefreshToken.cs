using System;
using Zn.Domain.Entity.Common;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// Bir kullanıcıya ait, rotation'lı refresh token kaydını temsil eder.
    /// BaseEntity'den Guid Id, CreatedAt ve UpdatedAt alanlarını miras alır.
    /// <para>
    /// Yaşam döngüsü: login'de üretilir; her /refresh çağrısında eski token revoke edilir,
    /// <see cref="ReplacedByToken"/> ile yenisine zincirlenir (rotation). Revoke edilmiş bir
    /// token tekrar kullanılırsa replay saldırısı kabul edilir ve kullanıcının tüm aktif
    /// token'ları iptal edilir (bu mantık Application handler'ında yer alır).
    /// </para>
    /// </summary>
    public class RefreshToken : BaseEntity
    {
        /// <summary>
        /// Token'ın benzersiz değeri. Bu projede SHA-256 hash olarak saklanır
        /// (düz token istemciye döner, DB'de yalnızca hash tutulur — bkz. handler'lar).
        /// </summary>
        public string Token { get; set; } = null!;

        /// <summary>
        /// Foreign key: Token'ın sahibi kullanıcının Id'si.
        /// IdentityUser'ın anahtar tipi string olduğu için string'dir.
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>Navigation property: Token'ın sahibi kullanıcı (N RefreshToken - 1 User).</summary>
        public User User { get; set; } = null!;

        /// <summary>Token'ın UTC son geçerlilik anı.</summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>Token'ın revoke edildiği UTC an; revoke edilmemişse null.</summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Rotation'da bu token'ın yerini alan yeni token'ın hash'i; zincir takibi içindir.
        /// Henüz değiştirilmemişse null.
        /// </summary>
        public string? ReplacedByToken { get; set; }

        /// <summary>Token'ın süresi dolmuşsa true (hesaplanan, DB'ye eşlenmez).</summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        /// <summary>
        /// Token revoke edilmemiş ve süresi dolmamışsa true; yani kullanılabilir durumda
        /// (hesaplanan, DB'ye eşlenmez).
        /// </summary>
        public bool IsActive => RevokedAt is null && !IsExpired;
    }
}
