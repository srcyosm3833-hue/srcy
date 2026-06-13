namespace Zn.Application.Features.Roles.CreateRole
{
    /// <summary>
    /// Yeni bir özel rol oluşturma komutu. Yetki: yalnızca Admin (controller'da uygulanır).
    /// Aynı adda bir rol varsa 409; başarıda oluşturulan rolün temsili döner.
    /// </summary>
    /// <param name="Name">Oluşturulacak rolün benzersiz adı.</param>
    public sealed record CreateRoleCommand(string Name);
}
