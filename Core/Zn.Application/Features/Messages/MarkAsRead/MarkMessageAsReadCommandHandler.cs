using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Messages.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Messages.MarkAsRead
{
    /// <summary>
    /// <see cref="MarkMessageAsReadCommand"/>'ı işleyen Wolverine handler'ı. Mesaj yoksa NotFound
    /// (404); aksi halde <see cref="Message.MarkAsRead"/> mutator'ı ile okunma durumunu explicit
    /// olarak set edip kaydeder. Başarıda güncellenmiş mesajın yanıtı döner (200).
    /// </summary>
    public static class MarkMessageAsReadCommandHandler
    {
        public static async Task<Result<MessageResponse>> Handle(
            MarkMessageAsReadCommand command,
            IMessageRepository messageRepository,
            CancellationToken cancellationToken)
        {
            Message? message = await messageRepository.GetByIdAsync(command.Id, cancellationToken);
            if (message is null)
            {
                return Result.Failure<MessageResponse>(MessageErrors.NotFound(command.Id));
            }

            // Explicit set: gövdedeki IsRead değeri ne ise ona göre işaretlenir (true veya false).
            message.MarkAsRead(command.IsRead);

            await messageRepository.SaveChangesAsync(cancellationToken);

            MessageResponse response = MessageMapper.ToResponse(message);

            return Result.Success(response);
        }
    }
}
