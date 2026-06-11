using System;
using Zn.Domain.Entity.Common;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// Ziyaretçiden gelen iletişim mesajını temsil eder. BaseEntity'den
    /// Guid tipinde Id, CreatedAt ve UpdatedAt alanlarını miras alır.
    /// </summary>
    public class Message : BaseEntity
    {
        /// <summary>Gönderenin adı. Non-nullable.</summary>
        public string Name { get; set; } = null!;

        /// <summary>Gönderenin e-posta adresi. Non-nullable.</summary>
        public string Email { get; set; } = null!;

        /// <summary>Mesaj konusu. Non-nullable.</summary>
        public string Subject { get; set; } = null!;

        /// <summary>Mesaj içeriği. Non-nullable.</summary>
        public string MessageBody { get; set; } = null!;

        /// <summary>
        /// Mesajın okunup okunmadığı. Value-type (bool) olduğu için doğası gereği
        /// non-nullable'dır; yeni mesaj varsayılan olarak okunmamış (false) başlar.
        /// </summary>
        public bool IsRead { get; set; }
    }
}
