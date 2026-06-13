using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zn.Application.Features.Users.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;
using Zn.Persistence.Context;

namespace Zn.Persistence.Repositories
{
    /// <summary>
    /// <see cref="IUserRepository"/>'nin EF Core implementasyonu. Roller AspNetUserRoles ile
    /// AspNetRoles join'lenerek DB seviyesinde projekte edilir; navigation koleksiyonları belleğe
    /// yüklenmez. Soft delete edilmiş kullanıcıları da görebilmek için sorgular
    /// <c>IgnoreQueryFilters()</c> ile global query filter'ı bypass eder (bkz. <see cref="IUserRepository"/>).
    /// </summary>
    public sealed class UserRepository : IUserRepository
    {
        private readonly ZnBlogDbContext _context;

        public UserRepository(ZnBlogDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<(IReadOnlyList<UserListItem> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            bool includeDeleted,
            CancellationToken cancellationToken)
        {
            // Users DbSet'i Identity tarafından yönetilir; soft delete filter'ı User'a uygulandığından
            // varsayılan sorgu yalnızca aktif kullanıcıları döndürür. includeDeleted=true ise filtre
            // bypass edilir. AsNoTracking salt-okuma için.
            IQueryable<User> query = _context.Users.AsNoTracking();

            if (includeDeleted)
            {
                query = query.IgnoreQueryFilters();
            }

            int totalCount = await query.CountAsync(cancellationToken);

            // Sıralama ve sayfalama DB tarafında. Roller, AspNetUserRoles → AspNetRoles join'i ile
            // korelasyonlu alt sorgu olarak projekte edilir (her kullanıcı için tek tur DB'de hesaplanır).
            List<UserListItem> items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListItem(
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email!,
                    u.ImageUrl,
                    u.CreatedAt,
                    u.IsDeleted,
                    u.DeletedAt,
                    (from userRole in _context.UserRoles
                     join role in _context.Roles on userRole.RoleId equals role.Id
                     where userRole.UserId == u.Id
                     select role.Name!).ToList()))
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <inheritdoc />
        public async Task<UserListItem?> GetByIdAsync(string userId, CancellationToken cancellationToken)
        {
            // Filtresiz: güncelleme/silme sonrası soft delete edilmiş kullanıcının yanıtını da üretebilmek için.
            return await _context.Users
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(u => u.Id == userId)
                .Select(u => new UserListItem(
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email!,
                    u.ImageUrl,
                    u.CreatedAt,
                    u.IsDeleted,
                    u.DeletedAt,
                    (from userRole in _context.UserRoles
                     join role in _context.Roles on userRole.RoleId equals role.Id
                     where userRole.UserId == u.Id
                     select role.Name!).ToList()))
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> IsDeletedByEmailAsync(string email, CancellationToken cancellationToken)
        {
            // Filtresiz: silinmiş kullanıcıyı da görebilmek için. Kullanıcı yoksa false.
            // E-posta karşılaştırması normalize edilmiş alan üzerinden yapılır (Identity konvansiyonu).
            string normalized = email.ToUpperInvariant();

            return await _context.Users
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(u => u.NormalizedEmail == normalized)
                .Select(u => (bool?)u.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken) == true;
        }

        /// <inheritdoc />
        public async Task<string?> GetFullNameByIdAsync(string userId, CancellationToken cancellationToken)
        {
            // Filtresiz: arama logu snapshot'ı için kullanıcı soft delete edilmiş olsa bile çözülebilir.
            // Tam ad DB tarafında birleştirilir; yalnızca tek kolon çekilir (AsNoTracking + projeksiyon).
            return await _context.Users
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(u => u.Id == userId)
                .Select(u => u.FirstName + " " + u.LastName)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
