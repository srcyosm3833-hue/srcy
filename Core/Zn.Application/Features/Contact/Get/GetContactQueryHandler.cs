using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Contact.Common;
using Zn.Application.Interfaces.Persistence;

namespace Zn.Application.Features.Contact.Get
{
    /// <summary>
    /// <see cref="GetContactQuery"/>'i işleyen Wolverine handler'ı. Repository'den tekil iletişim
    /// kaydını DB seviyesinde projekte edilmiş olarak alır (FirstOrDefault). Kayıt yoksa NotFound
    /// (404) — bu, yönetici upsert ile ilk kaydı oluşturana kadar geçerli olan ilk-kurulum durumudur.
    /// </summary>
    public static class GetContactQueryHandler
    {
        public static async Task<Result<ContactResponse>> Handle(
            GetContactQuery query,
            IContactRepository contactRepository,
            CancellationToken cancellationToken)
        {
            ContactResponse? contact = await contactRepository.GetAsync(cancellationToken);
            if (contact is null)
            {
                return Result.Failure<ContactResponse>(ContactErrors.NotFound());
            }

            return Result.Success(contact);
        }
    }
}
