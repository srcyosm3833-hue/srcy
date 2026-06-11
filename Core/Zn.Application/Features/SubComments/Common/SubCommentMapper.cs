using Riok.Mapperly.Abstractions;

namespace Zn.Application.Features.SubComments.Common
{
    /// <summary>
    /// Riok.Mapperly kaynak-üretimli mapper'ı. Repository'nin DB seviyesinde projekte ettiği
    /// <see cref="SubCommentListItem"/> ara DTO'sunu dışa dönen <see cref="SubCommentResponse"/>'a
    /// eşler. Hesaplanan <c>IsEdited</c> alanı kaynak DTO'da bulunmadığı için özel bir eşleme
    /// metoduyla (<see cref="ComputeIsEdited"/>) UpdatedAt != null kuralından üretilir.
    /// </summary>
    [Mapper]
    public static partial class SubCommentMapper
    {
        /// <summary>Tek bir alt yorum projeksiyonunu yanıta eşler; IsEdited türetilir.</summary>
        [MapProperty(nameof(SubCommentListItem.UpdatedAt), nameof(SubCommentResponse.IsEdited), Use = nameof(ComputeIsEdited))]
        public static partial SubCommentResponse ToResponse(SubCommentListItem source);

        /// <summary>UpdatedAt'ten isEdited türetir: alt yorum en az bir kez güncellenmişse true.</summary>
        private static bool ComputeIsEdited(System.DateTime? updatedAt) => updatedAt is not null;
    }
}
