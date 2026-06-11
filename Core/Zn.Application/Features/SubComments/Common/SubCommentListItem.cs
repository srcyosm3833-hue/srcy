using System;

namespace Zn.Application.Features.SubComments.Common
{
    /// <summary>
    /// Repository'nin alt yorum okuma sorgusunda veritabanı seviyesinde projekte ettiği ara DTO.
    /// Yazar görünen adı (FirstName + " " + LastName) DB sorgusunda birleştirilerek tek alanda
    /// taşınır. <see cref="UpdatedAt"/>'ten türetilen <c>isEdited</c> Mapperly tarafından el
    /// yordamıyla hesaplanarak <see cref="SubCommentResponse"/>'a eşlenir.
    /// </summary>
    /// <param name="Id">Alt yorumun benzersiz kimliği.</param>
    /// <param name="SubCommentText">Alt yorum içeriği.</param>
    /// <param name="CommentId">Bağlı olduğu ana yorumun kimliği.</param>
    /// <param name="UserId">Alt yorumu yapan kullanıcının kimliği.</param>
    /// <param name="DisplayName">Yazarın görünen adı (FirstName + LastName, DB'de birleştirilir).</param>
    /// <param name="CreatedAt">Oluşturulma anı (UTC).</param>
    /// <param name="UpdatedAt">Son güncellenme anı (UTC); hiç düzenlenmediyse null.</param>
    public sealed record SubCommentListItem(
        Guid Id,
        string SubCommentText,
        Guid CommentId,
        string UserId,
        string DisplayName,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
