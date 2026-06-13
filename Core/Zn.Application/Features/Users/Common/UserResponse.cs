using System;
using System.Collections.Generic;

namespace Zn.Application.Features.Users.Common
{
    /// <summary>
    /// Admin kullanıcı yönetimi uçlarının dışa döndürdüğü kullanıcı temsili.
    /// Listeleme, güncelleme ve oluşturma sonrası yanıtlarda kullanılır.
    /// </summary>
    /// <param name="Id">Kullanıcının benzersiz kimliği.</param>
    /// <param name="FirstName">Kullanıcının adı.</param>
    /// <param name="LastName">Kullanıcının soyadı.</param>
    /// <param name="Email">Kullanıcının e-posta adresi.</param>
    /// <param name="ImageUrl">Profil görseli yolu/URL'i.</param>
    /// <param name="CreatedAt">Hesabın oluşturulma anı (UTC).</param>
    /// <param name="IsDeleted">Kullanıcı soft delete edilmişse true.</param>
    /// <param name="DeletedAt">Soft delete anı (UTC); aktif kullanıcıda null.</param>
    /// <param name="Roles">Kullanıcıya atanmış rollerin adları.</param>
    public sealed record UserResponse(
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
