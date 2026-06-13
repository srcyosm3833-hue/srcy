using System.Collections.Generic;
using Riok.Mapperly.Abstractions;

namespace Zn.Application.Features.Comments.GetAllForAdmin
{
    /// <summary>
    /// Riok.Mapperly kaynak-üretimli mapper'ı. Repository'nin veritabanı seviyesinde projekte ettiği
    /// <see cref="CommentModerationItem"/> ara DTO'sunu dışa dönen <see cref="CommentModerationResponse"/>'a
    /// eşler. İki tip alan adları birebir aynı olduğundan ek eşleme kuralı gerekmez; entity doğrudan
    /// eşlenmez (Categories/Comments dilimlerinin projeksiyon-DTO desenine paralel).
    /// </summary>
    [Mapper]
    public static partial class CommentModerationMapper
    {
        /// <summary>Tek bir moderasyon projeksiyonunu yanıta eşler.</summary>
        public static partial CommentModerationResponse ToResponse(CommentModerationItem source);

        /// <summary>Moderasyon projeksiyonu koleksiyonunu yanıt listesine eşler.</summary>
        public static partial IReadOnlyList<CommentModerationResponse> ToResponseList(
            IReadOnlyList<CommentModerationItem> source);
    }
}
