using System;

namespace Zn.Application.Features.Categories.GetById
{
    /// <summary>
    /// Tek bir kategoriyi Id ile getiren sorgu (herkese açık). Bulunamazsa NotFound (404).
    /// Başarıda <see cref="Common.CategoryResponse"/> döner.
    /// </summary>
    /// <param name="Id">Getirilecek kategorinin kimliği.</param>
    public sealed record GetCategoryByIdQuery(Guid Id);
}
