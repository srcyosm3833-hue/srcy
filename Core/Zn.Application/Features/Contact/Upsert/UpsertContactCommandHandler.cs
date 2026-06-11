using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Contact.Common;
using Zn.Application.Interfaces.Persistence;
using DomainContact = Zn.Domain.Entity.Contact;

namespace Zn.Application.Features.Contact.Upsert
{
    /// <summary>
    /// <see cref="UpsertContactCommand"/>'ı işleyen Wolverine handler'ı. Tekil iletişim kaydını
    /// yönetir: mevcut kayıt yoksa <see cref="DomainContact.Create"/> ile oluşturup ekler
    /// (WasCreated=true → controller 201), varsa <see cref="DomainContact.Update"/> mutator'ı ile
    /// günceller (WasCreated=false → controller 200).
    /// <para>
    /// İKİNCİ KAYIT GARANTİSİ: Insert yalnızca mevcut kayıt null ise yapılır; aksi halde her zaman
    /// mevcut entity üzerinde Update çağrılır. Böylece tabloda hiçbir zaman birden fazla Contact satırı oluşmaz.
    /// </para>
    /// </summary>
    public static class UpsertContactCommandHandler
    {
        public static async Task<Result<UpsertContactResult>> Handle(
            UpsertContactCommand command,
            IContactRepository contactRepository,
            CancellationToken cancellationToken)
        {
            // Mevcut tekil kaydı tracked olarak getir. Null ise insert, değilse update yolu izlenir.
            DomainContact? existing = await contactRepository.GetTrackedAsync(cancellationToken);

            bool wasCreated;
            DomainContact contact;

            if (existing is null)
            {
                // İlk kurulum: tek kayıt henüz yok → oluştur ve ekle.
                contact = DomainContact.Create(
                    command.Address,
                    command.Email,
                    command.Phone,
                    command.MapUrl);

                await contactRepository.AddAsync(contact, cancellationToken);
                wasCreated = true;
            }
            else
            {
                // Kayıt zaten var → ikinci satır oluşturmadan mevcut entity'yi güncelle.
                existing.Update(
                    command.Address,
                    command.Email,
                    command.Phone,
                    command.MapUrl);

                contact = existing;
                wasCreated = false;
            }

            await contactRepository.SaveChangesAsync(cancellationToken);

            ContactResponse response = ContactMapper.ToResponse(contact);

            return Result.Success(new UpsertContactResult(response, wasCreated));
        }
    }
}
