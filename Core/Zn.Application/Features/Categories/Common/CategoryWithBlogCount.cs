using System;

namespace Zn.Application.Features.Categories.Common
{
    /// <summary>
    /// Repository'nin liste/tekil okuma sorgularında veritabanı seviyesinde projekte ettiği
    /// ara DTO. Category entity'sinin alanlarına ek olarak hesaplanan blog sayısını taşır;
    /// böylece kategorileri çekip belleğe Blogs koleksiyonunu yüklemeye gerek kalmaz.
    /// Mapperly bu tipi <see cref="CategoryResponse"/>'a eşler.
    /// </summary>
    /// <param name="Id">Kategorinin benzersiz kimliği.</param>
    /// <param name="CategoryName">Kategori adı.</param>
    /// <param name="BlogCount">Bu kategoriye bağlı blog sayısı (COUNT ile DB'de hesaplanır).</param>
    /// <param name="CreatedAt">Oluşturulma anı (UTC).</param>
    /// <param name="UpdatedAt">Son güncellenme anı (UTC); hiç güncellenmediyse null.</param>
    public sealed record CategoryWithBlogCount(
        Guid Id,
        string CategoryName,
        int BlogCount,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
