using System;

namespace Zn.Application.Features.SubComments.Common
{
    /// <summary>
    /// Dışa dönen alt yorum yanıtı. <see cref="IsEdited"/>, alt yorumun hiç güncellenip
    /// güncellenmediğini gösterir ve <see cref="UpdatedAt"/> != null kuralından türetilir
    /// (entity yalnızca <c>Update()</c> mutator'ında UpdatedAt set ettiği için epsilon gerekmez).
    /// </summary>
    /// <param name="Id">Alt yorumun benzersiz kimliği.</param>
    /// <param name="SubCommentText">Alt yorum içeriği.</param>
    /// <param name="CommentId">Bağlı olduğu ana yorumun kimliği.</param>
    /// <param name="UserId">Alt yorumu yapan kullanıcının kimliği.</param>
    /// <param name="DisplayName">Yazarın görünen adı (FirstName + LastName).</param>
    /// <param name="CreatedAt">Oluşturulma anı (UTC).</param>
    /// <param name="UpdatedAt">Son güncellenme anı (UTC); hiç düzenlenmediyse null.</param>
    /// <param name="IsEdited">Alt yorum en az bir kez düzenlendi mi (UpdatedAt != null).</param>
    public sealed record SubCommentResponse(
        Guid Id,
        string SubCommentText,
        Guid CommentId,
        string UserId,
        string DisplayName,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        bool IsEdited);
}
