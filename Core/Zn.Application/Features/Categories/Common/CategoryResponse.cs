using System;

namespace Zn.Application.Features.Categories.Common
{
    /// <summary>
    /// Kategori API yanıtı. Entity yerine dışarıya dönen sözleşmedir; navigation
    /// koleksiyonları sızdırmaz. <see cref="BlogCount"/> frontend'de kategori başına
    /// blog adedini göstermek için doludur.
    /// </summary>
    /// <param name="Id">Kategorinin benzersiz kimliği.</param>
    /// <param name="CategoryName">Kategori adı.</param>
    /// <param name="BlogCount">Bu kategoriye bağlı blog sayısı.</param>
    /// <param name="CreatedAt">Oluşturulma anı (UTC).</param>
    /// <param name="UpdatedAt">Son güncellenme anı (UTC); hiç güncellenmediyse null.</param>
    public sealed record CategoryResponse(
        Guid Id,
        string CategoryName,
        int BlogCount,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
