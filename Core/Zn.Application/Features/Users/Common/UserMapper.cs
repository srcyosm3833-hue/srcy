using System.Collections.Generic;
using Riok.Mapperly.Abstractions;

namespace Zn.Application.Features.Users.Common
{
    /// <summary>
    /// Riok.Mapperly kaynak-üretimli mapper'ı. Repository'nin DB seviyesinde projekte ettiği
    /// <see cref="UserListItem"/> projeksiyon DTO'sunu dışa dönen <see cref="UserResponse"/>'a eşler.
    /// Eşleme kodu derleme zamanında üretilir (reflection yoktur). User entity'si doğrudan
    /// eşlenmez; roller AspNetUserRoles üzerinden ayrıca projekte edilip DTO ile taşınır.
    /// </summary>
    [Mapper]
    public static partial class UserMapper
    {
        /// <summary>Tek bir projeksiyon DTO'sunu API yanıtına eşler.</summary>
        public static partial UserResponse ToResponse(UserListItem source);

        /// <summary>Projeksiyon DTO listesini API yanıt listesine eşler.</summary>
        public static partial IReadOnlyList<UserResponse> ToResponseList(
            IReadOnlyList<UserListItem> source);
    }
}
