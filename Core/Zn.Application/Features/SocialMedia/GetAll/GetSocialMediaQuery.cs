namespace Zn.Application.Features.SocialMedia.GetAll
{
    /// <summary>
    /// Tüm sosyal medya bağlantılarını getiren sorgu. Herkese açıktır ve parametre taşımaz.
    /// Liste küçük olduğundan sayfalama yoktur. Kayıt yoksa boş liste döner (404 değil).
    /// Başarıda <see cref="Common.SocialMediaResponse"/> listesi döner.
    /// </summary>
    public sealed record GetSocialMediaQuery;
}
