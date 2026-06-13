namespace Zn.Application.Features.Users.SoftDeleteUser
{
    /// <summary>
    /// Bir kullanıcıyı soft delete eden komut (A8: kullanıcının blogları silinmez/anonimleştirilmez,
    /// yalnızca hesap devre dışı kalır). Yetki: yalnızca Admin.
    /// </summary>
    /// <param name="TargetUserId">Silinecek kullanıcının kimliği (route'tan).</param>
    /// <param name="RequestingUserId">İsteği yapan (Admin) kullanıcının kimliği (token'dan).
    /// Kendini silme engeli için karşılaştırmada kullanılır.</param>
    public sealed record SoftDeleteUserCommand(string TargetUserId, string RequestingUserId);
}
