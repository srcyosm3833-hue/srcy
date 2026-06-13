using System.Collections.Generic;
using Riok.Mapperly.Abstractions;

namespace Zn.Application.Features.Blogs.Common
{
    /// <summary>
    /// Riok.Mapperly kaynak-üretimli mapper'ı. Repository'nin DB seviyesinde projekte ettiği
    /// ara DTO'ları (<see cref="BlogListItem"/>, <see cref="BlogDetail"/>) dışa dönen
    /// response'lara eşler. Yazar adı projeksiyonda zaten birleştirildiği için eşleme düzdür.
    /// Reflection yoktur; eşleme kodu derleme zamanında üretilir (partial method gövdeleri
    /// generator tarafından doldurulur).
    /// </summary>
    [Mapper]
    public static partial class BlogMapper
    {
        /// <summary>Tek bir liste projeksiyonunu liste yanıt öğesine eşler.</summary>
        public static partial BlogListItemResponse ToListItemResponse(BlogListItem source);

        /// <summary>Liste projeksiyonu koleksiyonunu liste yanıtına eşler.</summary>
        public static partial IReadOnlyList<BlogListItemResponse> ToListItemResponseList(
            IReadOnlyList<BlogListItem> source);

        /// <summary>Detay projeksiyonunu detay yanıtına eşler.</summary>
        public static partial BlogDetailResponse ToDetailResponse(BlogDetail source);

        /// <summary>Admin audit detay projeksiyonunu audit detay yanıtına (CreatorIpHash dahil) eşler.</summary>
        public static partial BlogAuditDetailResponse ToAuditDetailResponse(BlogAuditDetail source);
    }
}
