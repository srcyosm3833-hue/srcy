using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Messages.Send
{
    /// <summary>
    /// <see cref="SendMessageCommand"/>'ı işleyen Wolverine handler'ı (plain metot konvansiyonu).
    /// <see cref="Message.Create"/> factory'si ile invariant'lara uygun, okunmamış (IsRead=false)
    /// bir mesaj oluşturup kaydeder. Yanıt değer taşımaz: ziyaretçiye yalnızca onay döner
    /// (controller 201 olarak sunar); oluşturulan kaydın Id'si bilinçli olarak paylaşılmaz.
    /// </summary>
    public static class SendMessageCommandHandler
    {
        public static async Task<Result> Handle(
            SendMessageCommand command,
            IMessageRepository messageRepository,
            CancellationToken cancellationToken)
        {
            // Invariant'lar (boş olmayan alanlar, azami uzunluk) Domain factory'sinde korunur;
            // mesaj okunmamış başlar.
            Message message = Message.Create(
                command.Name,
                command.Email,
                command.Subject,
                command.MessageBody);

            await messageRepository.AddAsync(message, cancellationToken);
            await messageRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
