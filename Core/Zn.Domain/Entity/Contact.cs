using System;
using Zn.Domain.Entity.Common;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// İletişim bilgilerini temsil eder. BaseEntity'den Guid tipinde Id,
    /// CreatedAt ve UpdatedAt alanlarını miras alır.
    /// </summary>
    public class Contact : BaseEntity
    {
        /// <summary>Açık adres. Non-nullable.</summary>
        public string Address { get; set; } = null!;

        /// <summary>E-posta adresi. Non-nullable.</summary>
        public string Email { get; set; } = null!;

        /// <summary>Telefon numarası. Non-nullable.</summary>
        public string Phone { get; set; } = null!;

        /// <summary>Harita (embed/konum) URL'i. Non-nullable.</summary>
        public string MapUrl { get; set; } = null!;
    }
}
