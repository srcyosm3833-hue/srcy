using System;

namespace Zn.Application.Features.Blogs.Common
{
    /// <summary>
    /// Repository'nin admin audit detay sorgusunda veritabanı seviyesinde projekte ettiği ara DTO.
    /// Public <see cref="BlogDetail"/>'in tüm alanlarını taşır VE ek olarak audit alanı
    /// <see cref="CreatorIpHash"/>'i içerir. Mapperly bunu <see cref="BlogAuditDetailResponse"/>'a
    /// birebir eşler. Bu tip yalnızca Admin sorgu yolunda kullanılır; public yola sızmaz.
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
    /// <param name="LikeCount">Blogun toplam beğeni sayısı.</param>
    /// <param name="CreatorIpHash">Oluşturanın tuzlu SHA-256 IP hash'i (audit); çözülemediyse null.</param>
    public sealed record BlogAuditDetail(
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
        string? CreatorIpHash);
}
