using System;

namespace Zn.Application.Features.Blogs.Common
{
    /// <summary>
    /// Repository'nin liste sorgusunda veritabanı seviyesinde projekte ettiği ara DTO.
    /// Yazar adı (FirstName + " " + LastName) DB sorgusunda birleştirilerek tek alanda taşınır;
    /// Mapperly bu tipi düz biçimde <see cref="BlogListItemResponse"/>'a eşler.
    /// Büyük alanlar (Description, BlogImage) bilinçli olarak DB'den çekilmez.
    /// </summary>
    /// <param name="Id">Blogun benzersiz kimliği.</param>
    /// <param name="Title">Blog başlığı.</param>
    /// <param name="CoverImage">Kapak görseli URL'i.</param>
    /// <param name="CategoryId">Bağlı kategorinin kimliği.</param>
    /// <param name="CategoryName">Bağlı kategorinin adı.</param>
    /// <param name="AuthorName">Yazarın tam adı (DB'de birleştirilir).</param>
    /// <param name="CreatedAt">Oluşturulma anı (UTC).</param>
    /// <param name="LikeCount">Blogun toplam beğeni sayısı (DB'de COUNT ile hesaplanır).</param>
    /// <param name="IsLikedByCurrentUser">İsteği yapan kullanıcı bu blogu beğenmiş mi (anonimde false).</param>
    public sealed record BlogListItem(
        Guid Id,
        string Title,
        string CoverImage,
        Guid CategoryId,
        string CategoryName,
        string AuthorName,
        DateTime CreatedAt,
        int LikeCount,
        bool IsLikedByCurrentUser);
}
