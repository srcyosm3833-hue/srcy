using System;

namespace Zn.Application.Features.Categories.Update
{
    /// <summary>
    /// Var olan bir kategoriyi güncelleme komutu (admin). Bulunamazsa NotFound (404),
    /// hedef ad başka bir kategoride kullanılıyorsa Conflict (409). Başarıda güncel
    /// <see cref="Common.CategoryResponse"/> döner.
    /// </summary>
    /// <param name="Id">Güncellenecek kategorinin kimliği (route'tan gelir).</param>
    /// <param name="CategoryName">Kategorinin yeni adı.</param>
    public sealed record UpdateCategoryCommand(Guid Id, string CategoryName);
}
