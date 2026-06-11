using System;

namespace Zn.Application.Features.Blogs.Common
{
    /// <summary>
    /// Tek bir blogun tam detayını dışa dönen yanıt. Liste yanıtının aksine ağır alanları
    /// (Description, BlogImage) ve genişletilmiş yazar/kategori bilgisini içerir.
    /// </summary>
    /// <param name="Id">Blogun benzersiz kimliği.</param>
    /// <param name="Title">Blog başlığı.</param>
    /// <param name="CoverImage">Kapak görseli URL'i.</param>
    /// <param name="BlogImage">İçerik görseli URL'i.</param>
    /// <param name="Description">Blog içeriği/açıklaması.</param>
    /// <param name="CategoryId">Bağlı kategorinin kimliği.</param>
    /// <param name="CategoryName">Bağlı kategorinin adı.</param>
    /// <param name="AuthorId">Yazarın kullanıcı kimliği.</param>
    /// <param name="AuthorName">Yazarın tam adı (FirstName + LastName).</param>
    /// <param name="CreatedAt">Oluşturulma anı (UTC).</param>
    /// <param name="UpdatedAt">Son güncellenme anı (UTC); hiç güncellenmediyse null.</param>
    public sealed record BlogDetailResponse(
        Guid Id,
        string Title,
        string CoverImage,
        string BlogImage,
        string Description,
        Guid CategoryId,
        string CategoryName,
        string AuthorId,
        string AuthorName,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
