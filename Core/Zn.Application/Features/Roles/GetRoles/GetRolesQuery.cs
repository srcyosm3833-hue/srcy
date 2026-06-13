namespace Zn.Application.Features.Roles.GetRoles
{
    /// <summary>
    /// Sistemdeki tüm rolleri, her rol için atanmış kullanıcı sayısıyla birlikte listeleyen sorgu.
    /// Yetki: yalnızca Admin (controller'da uygulanır). Sayfalama yoktur; rol sayısı sınırlıdır.
    /// </summary>
    public sealed record GetRolesQuery();
}
