using System;
using Zn.Domain.Entity.Common;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// Sosyal medya hesabı bağlantısını temsil eder. BaseEntity'den
    /// Guid tipinde Id, CreatedAt ve UpdatedAt alanlarını miras alır.
    /// </summary>
    public class SocialMedia : BaseEntity
    {
        /// <summary>Platform adı (Instagram, X, LinkedIn vb.). Non-nullable.</summary>
        public string Title { get; set; } = null!;

        /// <summary>Profil/hesap bağlantısı. Non-nullable.</summary>
        public string Url { get; set; } = null!;

        /// <summary>İkon CSS sınıfı veya ikon dosya yolu. Non-nullable.</summary>
        public string Icon { get; set; } = null!;
    }
}
