using System;
using System.Collections.Generic;

namespace Zn.Application.Interfaces.Authentication
{
    /// <summary>
    /// Access token üretimi için gereken minimum kullanıcı bilgisini taşıyan girdi nesnesi.
    /// Bilinçli olarak Domain entity'lerine (User, RefreshToken) bağımlı değildir;
    /// böylece token servisi Application sözleşmesine bağlı kalır ve Faz 1'de
    /// handler'lar bu nesneyi UserManager'dan okudukları verilerle doldurabilir.
    /// </summary>
    /// <param name="UserId">Kullanıcının kimliği (AspNetUsers.Id, string).</param>
    /// <param name="Email">Kullanıcının e-posta adresi; claim olarak gömülür.</param>
    /// <param name="UserName">Kullanıcı adı; claim olarak gömülür.</param>
    /// <param name="Roles">Kullanıcının rolleri; her biri ayrı bir role claim'i olur.</param>
    public sealed record TokenUser(
        string UserId,
        string Email,
        string UserName,
        IReadOnlyCollection<string> Roles);
}
