using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zn.Application.Features.Contact.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Persistence.Context;
using DomainContact = Zn.Domain.Entity.Contact;

namespace Zn.Persistence.Repositories
{
    /// <summary>
    /// <see cref="IContactRepository"/>'nin EF Core implementasyonu. Uygulama tek bir Contact kaydı
    /// tutar (upsert ile yönetilir). Okuma (<see cref="GetAsync"/>) AsNoTracking + DB seviyesinde
    /// projeksiyonla FirstOrDefault döner; upsert akışında yapılan okuma
    /// (<see cref="GetTrackedAsync"/>) mevcut kaydı mutasyon için tracked döner.
    /// </summary>
    public sealed class ContactRepository : IContactRepository
    {
        private readonly ZnBlogDbContext _context;

        public ContactRepository(ZnBlogDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<ContactResponse?> GetAsync(CancellationToken cancellationToken)
        {
            // Projeksiyon DB seviyesinde; tek kayıt olduğu için FirstOrDefault yeterlidir.
            return await _context.Contacts
                .AsNoTracking()
                .Select(c => new ContactResponse(
                    c.Id,
                    c.Address,
                    c.Email,
                    c.Phone,
                    c.MapUrl))
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<DomainContact?> GetTrackedAsync(CancellationToken cancellationToken)
        {
            // Tracked: upsert handler'ı var olan kaydı Update ile günceller; yoksa yeni kayıt ekler.
            return await _context.Contacts
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddAsync(DomainContact contact, CancellationToken cancellationToken)
        {
            await _context.Contacts.AddAsync(contact, cancellationToken);
        }

        /// <inheritdoc />
        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
