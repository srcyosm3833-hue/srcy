using System;

namespace Zn.Application.Features.Comments.GetAllForAdmin
{
    /// <summary>
    /// Admin moderasyon listesinin DÜZ (flat) ara projeksiyon DTO'su: her öğe ya bir yorum
    /// (<see cref="IsReply"/> = false) ya da bir alt yorumdur (<see cref="IsReply"/> = true).
    /// Repository, Comments ve SubComments tablolarını ayrı projeksiyonlarla bu ortak şekle çevirip
    /// veritabanı seviyesinde birleştirir (Concat) — böylece tek bir sıralı + sayfalı küme oluşur.
    /// <para>
    /// Yazar görünen adı (FirstName + " " + LastName) SQL tarafında birleştirilir. Frontend mevcut
    /// silme route'larını çağırabilsin diye yorumda <see cref="BlogId"/>, alt yorumda
    /// <see cref="ParentCommentId"/> mutlaka taşınır.
    /// </para>
    /// </summary>
    /// <param name="Id">Öğenin kimliği (yorumun ya da alt yorumun Id'si).</param>
    /// <param name="IsReply">Öğe bir alt yorum mu (true) yoksa üst düzey yorum mu (false).</param>
    /// <param name="BlogId">Öğenin (yorum ya da alt yorumun) ait olduğu blogun kimliği.</param>
    /// <param name="BlogTitle">Öğenin ait olduğu blogun başlığı.</param>
    /// <param name="UserId">Öğeyi yazan kullanıcının kimliği.</param>
    /// <param name="AuthorName">Yazarın görünen adı (FirstName + LastName, DB'de birleştirilir).</param>
    /// <param name="Text">Yorum/alt yorum içeriği.</param>
    /// <param name="CreatedAt">Oluşturulma anı (UTC).</param>
    /// <param name="ParentCommentId">Alt yorumsa bağlı olduğu üst yorumun Id'si; üst düzey yorumda null.</param>
    public sealed record CommentModerationItem(
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
