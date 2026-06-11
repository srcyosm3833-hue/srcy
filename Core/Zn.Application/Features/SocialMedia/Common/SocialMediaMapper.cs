using System.Collections.Generic;
using Riok.Mapperly.Abstractions;
using DomainSocialMedia = Zn.Domain.Entity.SocialMedia;

namespace Zn.Application.Features.SocialMedia.Common
{
    /// <summary>
    /// Riok.Mapperly kaynak-üretimli (source-generated) mapper'ı. Reflection yoktur;
    /// eşleme kodu derleme zamanında üretilir (partial method gövdeleri generator tarafından doldurulur).
    /// <para>
    /// İki kaynaktan eşler: liste akışında DB'de projekte edilen <see cref="SocialMediaListItem"/>,
    /// create/update akışında ise elde kalan tracked <see cref="DomainSocialMedia"/> entity'si
    /// (alan adları birebir örtüştüğü için doğrudan eşlenir).
    /// </para>
    /// <para>
    /// Feature namespace'i (<c>Zn.Application.Features.SocialMedia</c>) entity'nin kısa adıyla
    /// (<c>SocialMedia</c>) çakıştığından <c>DomainSocialMedia</c> alias'ı kullanılır.
    /// </para>
    /// </summary>
    [Mapper]
    public static partial class SocialMediaMapper
    {
        /// <summary>Liste projeksiyon DTO listesini API yanıt listesine eşler.</summary>
        public static partial IReadOnlyList<SocialMediaResponse> ToResponseList(
            IReadOnlyList<SocialMediaListItem> source);

        /// <summary>Tek bir entity'yi API yanıtına eşler (create/update sonrası).</summary>
        public static partial SocialMediaResponse ToResponse(DomainSocialMedia source);
    }
}
