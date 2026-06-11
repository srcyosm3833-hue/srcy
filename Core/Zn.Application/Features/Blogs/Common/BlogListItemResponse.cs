using System;

namespace Zn.Application.Features.Blogs.Common
{
    /// <summary>
    /// Blog liste (sayfalı) yanıtındaki tek bir öğe. Listede büyük alanlar (Description,
    /// BlogImage) taşınmaz; yalnızca kart/satır görünümü için gereken alanlar döner.
    /// </summary>
    /// <param name="Id">Blogun benzersiz kimliği.</param>
    /// <param name="Title">Blog başlığı.</param>
    /// <param name="CoverImage">Kapak görseli URL'i.</param>
    /// <param name="CategoryId">Bağlı kategorinin kimliği.</param>
    /// <param name="CategoryName">Bağlı kategorinin adı.</param>
    /// <param name="AuthorName">Yazarın tam adı (FirstName + LastName).</param>
    /// <param name="CreatedAt">Oluşturulma anı (UTC).</param>
    public sealed record BlogListItemResponse(
        Guid Id,
        string Title,
        string CoverImage,
        Guid CategoryId,
        string CategoryName,
        string AuthorName,
        DateTime CreatedAt);
}
