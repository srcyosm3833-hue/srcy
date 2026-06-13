using System;

namespace Zn.Application.Features.Comments.GetAllForAdmin
{
    /// <summary>
    /// Admin yorum moderasyon listesinin dışa dönen DÜZ (flat) satır yanıtı: her satır ya bir yorum
    /// ya da bir alt yorumdur (<see cref="IsReply"/> ile ayrılır). Frontend bu satırdan mevcut silme
    /// uçlarını çağırır:
    /// <list type="bullet">
    /// <item>Yorum (IsReply=false) → <c>DELETE /api/blogs/{BlogId}/comments/{Id}</c>.</item>
    /// <item>Alt yorum (IsReply=true) → <c>DELETE /api/comments/{ParentCommentId}/replies/{Id}</c>.</item>
    /// </list>
    /// </summary>
    /// <param name="Id">Öğenin kimliği (yorumun ya da alt yorumun Id'si).</param>
    /// <param name="IsReply">Öğe bir alt yorum mu (true) yoksa üst düzey yorum mu (false).</param>
    /// <param name="BlogId">Öğenin ait olduğu blogun kimliği (silme route'u için zorunlu).</param>
    /// <param name="BlogTitle">Öğenin ait olduğu blogun başlığı (moderasyon listesinde gösterim için).</param>
    /// <param name="UserId">Yorumu/alt yorumu yazan kullanıcının kimliği.</param>
    /// <param name="AuthorName">Yazarın görünen adı (FirstName + LastName).</param>
    /// <param name="Text">Yorum/alt yorum içeriği.</param>
    /// <param name="CreatedAt">Oluşturulma anı (UTC).</param>
    /// <param name="ParentCommentId">Alt yorumsa bağlı üst yorumun Id'si (silme route'u için); üst düzey yorumda null.</param>
    public sealed record CommentModerationResponse(
        Guid Id,
        bool IsReply,
        Guid BlogId,
        string BlogTitle,
        string UserId,
        string AuthorName,
        string Text,
        DateTime CreatedAt,
        Guid? ParentCommentId);
}
