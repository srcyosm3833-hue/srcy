namespace Zn.Application.Features.Contact.Get
{
    /// <summary>
    /// Tekil iletişim bilgisi kaydını getiren sorgu. Herkese açıktır (anonim ziyaretçi).
    /// Başarıda <see cref="Common.ContactResponse"/> döner. Henüz hiç iletişim kaydı
    /// yapılandırılmadıysa (ilk kurulum öncesi) NotFound (404) döner.
    /// </summary>
    public sealed record GetContactQuery;
}
