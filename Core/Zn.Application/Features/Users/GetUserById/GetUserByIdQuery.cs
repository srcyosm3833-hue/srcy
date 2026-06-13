namespace Zn.Application.Features.Users.GetUserById
{
    /// <summary>
    /// Tek bir kullanıcıyı kimliğiyle getiren sorgu (A6 yetki matrisi: tekil getirme Admin + Manager).
    /// Başarıda <see cref="Common.UserResponse"/>, kullanıcı yoksa NotFound döner.
    /// <para>
    /// Sorgu soft delete durumundan bağımsızdır (filtresiz): Admin/Manager, silinmiş bir kullanıcının
    /// detayını da görebilmelidir. Bu yüzden repository <c>IgnoreQueryFilters()</c> ile çalışır.
    /// </para>
    /// </summary>
    /// <param name="Id">Getirilecek kullanıcının benzersiz kimliği (string PK; IdentityUser).</param>
    public sealed record GetUserByIdQuery(string Id);
}
