namespace Zn.ClientWebApi.Seeding
{
    /// <summary>
    /// İlk admin kullanıcısının seed ayarları. "AdminUser" yapılandırma bölümünden bind edilir.
    /// Development değerleri appsettings.Development.json'da tutulur (gitignore'da);
    /// production'da user secrets / environment'tan gelir. Hiçbir gerçek şifre commit'lenmez.
    /// </summary>
    public sealed class AdminUserOptions
    {
        /// <summary>Bind edilecek yapılandırma bölümünün adı.</summary>
        public const string SectionName = "AdminUser";

        /// <summary>Seed edilecek admin kullanıcının e-postası. Boşsa admin seed atlanır.</summary>
        public string? Email { get; set; }

        /// <summary>Seed edilecek admin kullanıcının şifresi. Boşsa admin seed atlanır.</summary>
        public string? Password { get; set; }

        /// <summary>Admin kullanıcının adı (zorunlu User alanı). Verilmezse varsayılan kullanılır.</summary>
        public string FirstName { get; set; } = "System";

        /// <summary>Admin kullanıcının soyadı (zorunlu User alanı). Verilmezse varsayılan kullanılır.</summary>
        public string LastName { get; set; } = "Administrator";

        /// <summary>Admin kullanıcının profil görseli (zorunlu User alanı). Verilmezse varsayılan kullanılır.</summary>
        public string ImageUrl { get; set; } = "https://www.gravatar.com/avatar/?d=mp";

        /// <summary>E-posta ve şifre dolu mu? Dolu değilse admin kullanıcı seed'i atlanır.</summary>
        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
    }
}
