using System;

namespace Zn.Application.Features.Categories.Delete
{
    /// <summary>
    /// Bir kategoriyi silme komutu (admin). Bulunamazsa NotFound (404); kategoriye bağlı
    /// blog varsa Conflict (409). Başarıda değer taşımayan bir sonuç döner (HTTP 204).
    /// </summary>
    /// <param name="Id">Silinecek kategorinin kimliği.</param>
    public sealed record DeleteCategoryCommand(Guid Id);
}
