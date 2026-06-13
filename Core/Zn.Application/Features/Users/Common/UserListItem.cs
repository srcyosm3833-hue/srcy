using System;
using System.Collections.Generic;

namespace Zn.Application.Features.Users.Common
{
    /// <summary>
    /// Admin kullanıcı listesinde repository'nin veritabanı seviyesinde projekte ettiği ara DTO.
    /// User entity'sinin (IdentityUser) temel alanlarını taşır; kullanıcının rolleri
    /// AspNetUserRoles üzerinden DB seviyesinde projekte edilir. Mapperly bu tipi
    /// <see cref="UserResponse"/>'a eşler.
    /// </summary>
    /// <param name="Id">Kullanıcının benzersiz kimliği (string PK; IdentityUser).</param>
    /// <param name="FirstName">Kullanıcının adı.</param>
    /// <param name="LastName">Kullanıcının soyadı.</param>
    /// <param name="Email">Kullanıcının e-posta adresi.</param>
    /// <param name="ImageUrl">Profil görseli yolu/URL'i.</param>
    /// <param name="CreatedAt">Hesabın oluşturulma anı (UTC).</param>
    /// <param name="IsDeleted">Kullanıcı soft delete edilmişse true.</param>
    /// <param name="DeletedAt">Soft delete anı (UTC); aktif kullanıcıda null.</param>
    /// <param name="Roles">Kullanıcıya atanmış rollerin adları.</param>
    public sealed record UserListItem(
        string Id,
        string FirstName,
        string LastName,
        string Email,
        string ImageUrl,
        DateTime CreatedAt,
        bool IsDeleted,
        DateTime? DeletedAt,
        IReadOnlyList<string> Roles);
}
