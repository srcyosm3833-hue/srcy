namespace Zn.Application.Features.Categories.GetAll
{
    /// <summary>
    /// Tüm kategorileri (her birinde blog sayısıyla) getiren sorgu. Herkese açıktır;
    /// parametre taşımaz. Başarıda <see cref="Common.CategoryResponse"/> listesi döner.
    /// </summary>
    public sealed record GetCategoriesQuery;
}
