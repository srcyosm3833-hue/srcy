using System;
using Zn.Application.Common.Results;

namespace Zn.Application.Features.Messages.Common
{
    /// <summary>
    /// Message dikey diliminde tekrar eden hata tanımlarını tek noktada toplar.
    /// Handler'lar bu fabrikalar üzerinden anlamlı <see cref="Error"/> üretir;
    /// ApiControllerBase bunları uygun HTTP koduna eşler.
    /// </summary>
    public static class MessageErrors
    {
        /// <summary>Belirtilen Id'ye sahip mesaj bulunamadı (404).</summary>
        public static Error NotFound(Guid id) =>
            Error.NotFound("Message.NotFound", $"Message with id '{id}' was not found.");
    }
}
