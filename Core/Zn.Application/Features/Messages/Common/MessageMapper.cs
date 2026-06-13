using System.Collections.Generic;
using Riok.Mapperly.Abstractions;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Messages.Common
{
    /// <summary>
    /// Riok.Mapperly kaynak-üretimli mapper'ı. İki kaynaktan <see cref="MessageResponse"/> üretir:
    /// repository'nin DB seviyesinde projekte ettiği <see cref="MessageListItem"/> (listeleme) ve
    /// güncelleme sonrası elde kalan tracked <see cref="Message"/> entity'si (MarkAsRead yanıtı).
    /// Hesaplanan alan yoktur; tüm alanlar isim eşleşmesiyle birebir kopyalanır.
    /// </summary>
    [Mapper]
    public static partial class MessageMapper
    {
        /// <summary>Tek bir liste projeksiyonunu mesaj yanıtına eşler.</summary>
        public static partial MessageResponse ToResponse(MessageListItem source);

        /// <summary>Liste projeksiyonu koleksiyonunu mesaj yanıt listesine eşler.</summary>
        public static partial IReadOnlyList<MessageResponse> ToResponseList(
            IReadOnlyList<MessageListItem> source);

        /// <summary>
        /// Tracked Message entity'sini mesaj yanıtına eşler. Okunma durumu güncellendikten sonra
        /// güncel kaydı dönmek için kullanılır. Soft delete alanları (IsDeleted/DeletedAt) ve
        /// UpdatedAt yanıta taşınmaz; kaynakta yok sayılır.
        /// </summary>
        [MapperIgnoreSource(nameof(Message.IsDeleted))]
        [MapperIgnoreSource(nameof(Message.DeletedAt))]
        [MapperIgnoreSource(nameof(Message.UpdatedAt))]
        public static partial MessageResponse ToResponse(Message source);
    }
}
