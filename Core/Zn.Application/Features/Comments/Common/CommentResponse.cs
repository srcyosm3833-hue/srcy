using System;

namespace Zn.Application.Features.Comments.Common
{
    /// <summary>
    /// Dışa dönen yorum yanıtı. <see cref="IsEdited"/>, yorumun hiç güncellenip güncellenmediğini
    /// gösterir ve <see cref="UpdatedAt"/> != null kuralından türetilir (entity yalnızca
    /// <c>Update()</c> mutator'ında UpdatedAt set ettiği için epsilon/tolerans gerekmez).
    /// </summary>
    /// <param name="Id">Yorumun benzersiz kimliği.</param>
    /// <param name="CommentText">Yorum içeriği.</param>
    /// <param name="UserId">Yorumu yapan kullanıcının kimliği.</param>
    /// <param name="DisplayName">Yazarın görünen adı (FirstName + LastName).</param>
    /// <param name="CreatedAt">Oluşturulma anı (UTC).</param>
    /// <param name="UpdatedAt">Son güncellenme anı (UTC); hiç düzenlenmediyse null.</param>
    /// <param name="IsEdited">Yorum en az bir kez düzenlendi mi (UpdatedAt != null).</param>
    /// <param name="SubCommentCount">Bu yoruma bağlı alt yorum sayısı.</param>
    public sealed record CommentResponse(
        Guid Id,
        string CommentText,
        string UserId,
        string DisplayName,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        bool IsEdited,
        int SubCommentCount);
}
