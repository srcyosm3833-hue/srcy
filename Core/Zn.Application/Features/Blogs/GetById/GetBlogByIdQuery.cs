using System;

namespace Zn.Application.Features.Blogs.GetById
{
    /// <summary>
    /// Tek bir blogu Id ile tam detayıyla getiren sorgu (herkese açık).
    /// Bulunamazsa NotFound (404). Başarıda <see cref="Common.BlogDetailResponse"/> döner.
    /// </summary>
    /// <param name="Id">Getirilecek blogun kimliği.</param>
    public sealed record GetBlogByIdQuery(Guid Id);
}
