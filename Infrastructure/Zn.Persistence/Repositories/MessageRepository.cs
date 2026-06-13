using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zn.Application.Features.Messages.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;
using Zn.Persistence.Context;

namespace Zn.Persistence.Repositories
{
    /// <summary>
    /// <see cref="IMessageRepository"/>'nin EF Core implementasyonu. Listeleme sorgusu AsNoTracking +
    /// veritabanı seviyesinde projeksiyon kullanır; sıralama ve sayfalama DB tarafında uygulanır
    /// (okunmamışlar önce, ardından her grup içinde CreatedAt azalan). Okunma durumu güncellemesi
    /// için yapılan okuma tracked döner.
    /// </summary>
    public sealed class MessageRepository : IMessageRepository
    {
        private readonly ZnBlogDbContext _context;

        public MessageRepository(ZnBlogDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<(IReadOnlyList<MessageListItem> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            bool includeDeleted,
            CancellationToken cancellationToken)
        {
            IQueryable<Message> query = _context.Messages.AsNoTracking();

            // Admin/Manager sorgusu: soft delete edilmiş mesajları da görmek için global query
            // filter bypass edilir. Varsayılan (false) sorguda silinmiş mesajlar zaten dışlanır.
            if (includeDeleted)
            {
                query = query.IgnoreQueryFilters();
            }

            int totalCount = await query.CountAsync(cancellationToken);

            // Sıralama DB tarafında: önce okunmamışlar (IsRead=false → bool olarak küçük), ardından
            // her grup içinde en yeni mesaj önce. Projeksiyon DB seviyesinde yapılır.
            List<MessageListItem> items = await query
                .OrderBy(m => m.IsRead)
                .ThenByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MessageListItem(
                    m.Id,
                    m.Name,
                    m.Email,
                    m.Subject,
                    m.MessageBody,
                    m.IsRead,
                    m.CreatedAt,
                    m.SenderIpHash))
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <inheritdoc />
        public async Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            // Tracked: çağıran handler MarkAsRead uygular ve SaveChanges'le kalıcılaştırır.
            return await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddAsync(Message message, CancellationToken cancellationToken)
        {
            await _context.Messages.AddAsync(message, cancellationToken);
        }

        /// <inheritdoc />
        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
