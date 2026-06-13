using Riok.Mapperly.Abstractions;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Roles.Common
{
    /// <summary>
    /// Riok.Mapperly kaynak-üretimli mapper'ı. <see cref="Role"/> entity'sini dışa dönen
    /// <see cref="RoleResponse"/>'a eşler. <c>UserCount</c> ve <c>IsProtected</c> alanları source'ta
    /// bulunmadığından handler tarafından ek parametre olarak verilir; Mapperly bunları doğrudan
    /// hedefe yazar. Eşleme kodu derleme zamanında üretilir (reflection yoktur).
    /// </summary>
    [Mapper]
    public static partial class RoleMapper
    {
        /// <summary>
        /// Bir rol entity'sini, hesaplanan kullanıcı sayısı ve koruma bayrağıyla yanıta eşler.
        /// </summary>
        /// <param name="source">Identity rol entity'si (Id, Name buradan gelir).</param>
        /// <param name="userCount">Bu role atanmış kullanıcı sayısı (handler hesaplar).</param>
        /// <param name="isProtected">Rol sistem tarafından korunuyorsa true (handler belirler).</param>
        [MapperIgnoreSource(nameof(Role.NormalizedName))]
        [MapperIgnoreSource(nameof(Role.ConcurrencyStamp))]
        public static partial RoleResponse ToResponse(Role source, int userCount, bool isProtected);
    }
}
