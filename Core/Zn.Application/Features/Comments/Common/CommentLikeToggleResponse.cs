namespace Zn.Application.Features.Comments.Common
{
    /// <summary>
    /// Yorum beğeni toggle işleminin sonucunu dışa dönen yanıt.
    /// </summary>
    /// <param name="Liked">İşlem sonunda yorum mevcut kullanıcı tarafından beğenili mi (true = like, false = unlike).</param>
    /// <param name="LikeCount">İşlem sonrası yorumun toplam beğeni sayısı.</param>
    public sealed record CommentLikeToggleResponse(bool Liked, int LikeCount);
}
