using System;

namespace Zn.Application.Features.Blogs.GetById
{
    /// <summary>
    /// Tek bir blogu admin audit detayıyla (CreatorIpHash dahil) getiren sorgu. Yalnızca Admin/Manager
    /// erişimine açık <c>GET /api/admin/blogs/{id}</c> uç noktasından çağrılır; yetki controller
    /// seviyesinde uygulanır. Bulunamazsa NotFound (404). Başarıda
    /// <see cref="Common.BlogAuditDetailResponse"/> döner.
    /// <para>
    /// Public <see cref="GetBlogByIdQuery"/>'den ayrı tutulmuştur: audit alanı (CreatorIpHash)
    /// yalnızca bu sorgu yolundan döner, public yola hiçbir şekilde sızmaz.
    /// </para>
    /// </summary>
    /// <param name="Id">Getirilecek blogun kimliği.</param>
    public sealed record GetBlogAuditByIdQuery(Guid Id);
}
