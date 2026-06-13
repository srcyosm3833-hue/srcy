using System;

namespace Zn.Application.Features.Comments.Common
{
    /// <summary>
    /// Repository'nin liste sorgusunda veritabanı seviyesinde projekte ettiği ara DTO. Yazar
    /// görünen adı (FirstName + " " + LastName) DB sorgusunda birleştirilerek tek alanda taşınır;
    /// alt yorum sayısı (SubCommentCount) navigation koleksiyonu belleğe çekilmeden COUNT ile
    /// hesaplanır. <see cref="UpdatedAt"/>'ten türetilen <c>isEdited</c> Mapperly tarafından
    /// el yordamıyla hesaplanarak <see cref="CommentResponse"/>'a eşlenir.
    /// </summary>
    /// <param name="Id">Yorumun benzersiz kimliği.</param>
    /// <param name="CommentText">Yorum içeriği.</param>
    /// <param name="UserId">Yorumu yapan kullanıcının kimliği.</param>
    /// <param name="DisplayName">Yazarın görünen adı (FirstName + LastName, DB'de birleştirilir).</param>
    /// <param name="CreatedAt">Oluşturulma anı (UTC).</param>
    /// <param name="UpdatedAt">Son güncellenme anı (UTC); hiç düzenlenmediyse null.</param>
    /// <param name="SubCommentCount">Bu yoruma bağlı alt yorum sayısı (COUNT ile DB'de hesaplanır).</param>
    /// <param name="LikeCount">Yorumun toplam beğeni sayısı (COUNT ile DB'de hesaplanır).</param>
    /// <param name="IsLikedByCurrentUser">İsteği yapan kullanıcı bu yorumu beğenmiş mi (anonimde false).</param>
    public sealed record CommentListItem(
        Guid Id,
        string CommentText,
        string UserId,
        string DisplayName,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        int SubCommentCount,
        int LikeCount,
        bool IsLikedByCurrentUser);
}
