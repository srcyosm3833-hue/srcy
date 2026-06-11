using System;
using Zn.Application.Common.Results;

namespace Zn.Application.Features.SocialMedia.Common
{
    /// <summary>
    /// SocialMedia dikey diliminde tekrar eden hata tanımlarını tek noktada toplar.
    /// Handler'lar bu fabrikalar üzerinden anlamlı <see cref="Error"/> üretir;
    /// ApiControllerBase bunları uygun HTTP koduna eşler.
    /// </summary>
    public static class SocialMediaErrors
    {
        /// <summary>Belirtilen Id'ye sahip sosyal medya bağlantısı bulunamadı (404).</summary>
        public static Error NotFound(Guid id) =>
            Error.NotFound(
                "SocialMedia.NotFound",
                $"Social media link with id '{id}' was not found.");

        /// <summary>
        /// Aynı başlığa sahip bir sosyal medya bağlantısı zaten mevcut (409).
        /// Title üzerinde DB unique index'i son savunma hattıdır; handler bu anlamlı
        /// çakışmayı önceden döndürerek 500 yerine 409 verir (Category örüntüsü).
        /// </summary>
        public static Error TitleAlreadyExists(string title) =>
            Error.Conflict(
                "SocialMedia.TitleAlreadyExists",
                $"A social media link titled '{title}' already exists.");
    }
}
