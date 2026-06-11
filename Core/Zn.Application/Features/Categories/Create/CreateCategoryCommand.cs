namespace Zn.Application.Features.Categories.Create
{
    /// <summary>
    /// Yeni kategori oluşturma komutu (admin). Başarıda oluşturulan kategorinin
    /// <see cref="Common.CategoryResponse"/>'u döner. Immutable record.
    /// </summary>
    /// <param name="CategoryName">Oluşturulacak kategorinin adı.</param>
    public sealed record CreateCategoryCommand(string CategoryName);
}
