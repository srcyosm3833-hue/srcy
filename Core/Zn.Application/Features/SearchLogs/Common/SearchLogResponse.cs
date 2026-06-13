using System;

namespace Zn.Application.Features.SearchLogs.Common
{
    /// <summary>
    /// Admin arama log listesine dışa dönen yanıt. Repository DB seviyesinde doğrudan bu tipe
    /// projekte eder (ara DTO gerekmez; tüm alanlar entity'den birebir okunur). IP daima
    /// hash'lidir; ham IP hiçbir zaman dönmez.
    /// </summary>
    /// <param name="Id">Log kaydının benzersiz kimliği.</param>
    /// <param name="Term">Aranan terim.</param>
    /// <param name="UserId">Aramayı yapan kullanıcının kimliği; anonim aramada null.</param>
    /// <param name="UserFullName">Log anındaki kullanıcı tam adı snapshot'ı; anonimde null.</param>
    /// <param name="IpHash">Aramayı yapan istemcinin tuzlu SHA-256 IP hash'i; çözümlenemediyse null.</param>
    /// <param name="SearchedAt">Aramanın gerçekleştiği an (UTC).</param>
    public sealed record SearchLogResponse(
        Guid Id,
        string Term,
        string? UserId,
        string? UserFullName,
        string? IpHash,
        DateTime SearchedAt);
}
