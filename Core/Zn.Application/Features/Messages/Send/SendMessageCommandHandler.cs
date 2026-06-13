using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Interfaces.Audit;
using Zn.Application.Interfaces.Authentication;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Messages.Send
{
    /// <summary>
    /// <see cref="SendMessageCommand"/>'ı işleyen Wolverine handler'ı (plain metot konvansiyonu).
    /// <see cref="Message.Create"/> factory'si ile invariant'lara uygun, okunmamış (IsRead=false)
    /// bir mesaj oluşturup kaydeder. Yanıt değer taşımaz: ziyaretçiye yalnızca onay döner
    /// (controller 201 olarak sunar); oluşturulan kaydın Id'si bilinçli olarak paylaşılmaz.
    /// <para>
    /// Audit (anonim): Gönderenin IP adresi <see cref="IClientIpResolver"/> ile çözülür,
    /// <see cref="IIpHasher"/> ile tuzlu hash'lenir (ham IP saklanmaz) ve <see cref="Message.Create"/>
    /// factory'sine geçirilir. IP çözülemezse hash null olur ve mesaj yine de kaydedilir (hata yok).
    /// </para>
    /// </summary>
    public static class SendMessageCommandHandler
    {
        public static async Task<Result> Handle(
            SendMessageCommand command,
            IMessageRepository messageRepository,
            IClientIpResolver clientIpResolver,
            IIpHasher ipHasher,
            CancellationToken cancellationToken)
        {
            // İstemci IP'sini çöz ve saklamadan ÖNCE hash'le; çözülemezse null (audit opsiyonel,
            // mesaj yine de gönderilir). Ham IP hiçbir zaman entity'ye/DB'ye geçmez.
            string? ipHash = ipHasher.Hash(clientIpResolver.ResolveIpAddress());

            // Invariant'lar (boş olmayan alanlar, azami uzunluk) Domain factory'sinde korunur;
            // mesaj okunmamış başlar.
            Message message = Message.Create(
                command.Name,
                command.Email,
                command.Subject,
                command.MessageBody,
                ipHash);

            await messageRepository.AddAsync(message, cancellationToken);
            await messageRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
