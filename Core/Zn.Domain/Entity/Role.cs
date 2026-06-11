using System;
using Microsoft.AspNetCore.Identity;

namespace Zn.Domain.Entity
{
    /// <summary>
    /// Uygulama rolü. IdentityRole'den miras alır; IdentityRole'ün
    /// birincil anahtar tipi string'dir. Id ve Name alanları Identity'den gelir.
    /// İleride role özgü alan (örn. Description) eklemek için genişletme noktasıdır.
    /// </summary>
    public class Role : IdentityRole
    {
    }
}
