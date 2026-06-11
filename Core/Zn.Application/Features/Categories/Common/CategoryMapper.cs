using System.Collections.Generic;
using Riok.Mapperly.Abstractions;

namespace Zn.Application.Features.Categories.Common
{
    /// <summary>
    /// Riok.Mapperly kaynak-üretimli (source-generated) mapper'ı — projedeki ilk Mapperly kullanımı.
    /// <see cref="CategoryWithBlogCount"/> projeksiyon DTO'sunu dışa dönen
    /// <see cref="CategoryResponse"/>'a eşler. Reflection yoktur; eşleme kodu derleme
    /// zamanında üretilir (partial method gövdeleri generator tarafından doldurulur).
    /// <para>
    /// Entity (<c>Category</c>) doğrudan eşlenmez; çünkü blog sayısı veritabanında
    /// projekte edilip <see cref="CategoryWithBlogCount"/> ile taşınır. Bu, navigation
    /// koleksiyonlarını belleğe çekmeyi önler.
    /// </para>
    /// </summary>
    [Mapper]
    public static partial class CategoryMapper
    {
        /// <summary>Tek bir projeksiyon DTO'sunu API yanıtına eşler.</summary>
        public static partial CategoryResponse ToResponse(CategoryWithBlogCount source);

        /// <summary>Projeksiyon DTO listesini API yanıt listesine eşler.</summary>
        public static partial IReadOnlyList<CategoryResponse> ToResponseList(
            IReadOnlyList<CategoryWithBlogCount> source);
    }
}
