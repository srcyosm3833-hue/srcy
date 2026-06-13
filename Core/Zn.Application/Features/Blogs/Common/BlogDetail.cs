using System;

namespace Zn.Application.Features.Blogs.Common
{
    /// <summary>
    /// Repository'nin tekil (detay) okuma sorgusunda veritabanı seviyesinde projekte ettiği
    /// ara DTO. Yazar adı (FirstName + " " + LastName) DB sorgusunda birleştirilerek tek alanda
    /// taşınır; Mapperly bu tipi düz biçimde <see cref="BlogDetailResponse"/>'a eşler.
    /// </summary>
    /// <param name="Id">Blogun benzersiz kimliği.</param>
    /// <param name="Title">Blog başlığı.</param>
    /// <param name="CoverImage">Kapak görseli URL'i.</param>
    /// <param name="BlogImage">İçerik görseli URL'i.</param>
    /// <param name="Description">Blog içeriği/açıklaması.</param>
    /// <param name="CategoryId">Bağlı kategorinin kimliği.</param>
    /// <param name="CategoryName">Bağlı kategorinin adı.</param>
    /// <param name="AuthorId">Yazarın kullanıcı kimliği.</param>
    /// <param name="AuthorName">Yazarın tam adı (DB'de birleştirilir).</param>
    /// <param name="CreatedAt">Oluşturulma anı (UTC).</param>
    /// <param name="UpdatedAt">Son güncellenme anı (UTC); hiç güncellenmediyse null.</param>
    /// <param name="LikeCount">Blogun toplam beğeni sayısı (DB'de COUNT ile hesaplanır).</param>
    /// <param name="IsLikedByCurrentUser">İsteği yapan kullanıcı bu blogu beğenmiş mi (anonimde false).</param>
    public sealed record BlogDetail(
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
        DateTime? UpdatedAt,
        int LikeCount,
        bool IsLikedByCurrentUser);
}
