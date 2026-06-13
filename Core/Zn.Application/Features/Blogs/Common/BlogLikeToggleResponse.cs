namespace Zn.Application.Features.Blogs.Common
{
    /// <summary>
    /// Blog beğeni toggle işleminin sonucunu dışa dönen yanıt.
    /// </summary>
    /// <param name="Liked">İşlem sonunda blog mevcut kullanıcı tarafından beğenili mi (true = like, false = unlike).</param>
    /// <param name="LikeCount">İşlem sonrası blogun toplam beğeni sayısı.</param>
    public sealed record BlogLikeToggleResponse(bool Liked, int LikeCount);
}
