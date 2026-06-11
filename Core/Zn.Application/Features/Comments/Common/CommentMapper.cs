using System.Collections.Generic;
using Riok.Mapperly.Abstractions;

namespace Zn.Application.Features.Comments.Common
{
    /// <summary>
    /// Riok.Mapperly kaynak-üretimli mapper'ı. Repository'nin DB seviyesinde projekte ettiği
    /// <see cref="CommentListItem"/> ara DTO'sunu dışa dönen <see cref="CommentResponse"/>'a eşler.
    /// <para>
    /// Hesaplanan <c>IsEdited</c> alanı kaynak DTO'da bulunmadığı için bir özel eşleme metoduyla
    /// (<see cref="ComputeIsEdited"/>) UpdatedAt != null kuralından üretilir; Categories diliminin
    /// projeksiyon-DTO desenine paralel olarak entity doğrudan eşlenmez.
    /// </para>
    /// </summary>
    [Mapper]
    public static partial class CommentMapper
    {
        /// <summary>Tek bir liste projeksiyonunu yoruma eşler; IsEdited türetilir.</summary>
        [MapProperty(nameof(CommentListItem.UpdatedAt), nameof(CommentResponse.IsEdited), Use = nameof(ComputeIsEdited))]
        public static partial CommentResponse ToResponse(CommentListItem source);

        /// <summary>Liste projeksiyonu koleksiyonunu yorum yanıt listesine eşler.</summary>
        public static partial IReadOnlyList<CommentResponse> ToResponseList(
            IReadOnlyList<CommentListItem> source);

        /// <summary>UpdatedAt'ten isEdited türetir: yorum en az bir kez güncellenmişse true.</summary>
        private static bool ComputeIsEdited(System.DateTime? updatedAt) => updatedAt is not null;
    }
}
