using System.Collections.Generic;
using Zn.Application.Common.Results;

namespace Zn.Application.Features.Users.Common
{
    /// <summary>
    /// Admin kullanıcı yönetimi dikey diliminde tekrar eden hata tanımlarını tek noktada toplar.
    /// </summary>
    public static class UserErrors
    {
        /// <summary>Verilen Id'ye sahip kullanıcı bulunamadı (404).</summary>
        public static Error NotFound(string userId) =>
            Error.NotFound("User.NotFound", $"No user was found with id '{userId}'.");

        /// <summary>E-posta zaten kayıtlı (409).</summary>
        public static Error EmailAlreadyExists(string email) =>
            Error.Conflict("User.EmailAlreadyExists", $"A user with email '{email}' already exists.");

        /// <summary>Admin kendi hesabını silmeye çalıştı (400).</summary>
        public static readonly Error CannotDeleteSelf =
            Error.Validation("User.CannotDeleteSelf", "You cannot delete your own account.");

        /// <summary>
        /// Sistemdeki tek Admin'den Admin rolü kaldırılamaz (400). Sistemin yönetimsiz kalmasını önler.
        /// </summary>
        public static readonly Error LastAdminCannotLoseRole =
            Error.Validation(
                "User.LastAdminCannotLoseRole",
                "The Admin role cannot be removed from the last remaining administrator.");

        /// <summary>Identity'nin ürettiği kullanıcı oluşturma/güncelleme hataları (400).</summary>
        public static Error IdentityFailure(IReadOnlyDictionary<string, string[]> validations) =>
            Error.Validation("User.OperationFailed", "The user operation failed.", validations);
    }
}
