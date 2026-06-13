using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Features.Users.Common;
using Zn.Domain.Entity;

namespace Zn.Application.Interfaces.Persistence
{
    /// <summary>
    /// Kullanıcı okuma/yönetim kalıcılaştırma sözleşmesi. İmplementasyon Zn.Persistence'tadır.
    /// <para>
    /// <b>Neden ayrı bir repository?</b> User entity'si <see cref="Zn.Domain.Entity.Common.ISoftDeletable"/>
    /// uyguladığından AspNetUsers tablosuna global query filter (<c>!IsDeleted</c>) uygulanır ve
    /// <see cref="Microsoft.AspNetCore.Identity.UserManager{TUser}"/>'ın tüm sorguları (FindByEmail/FindById/Users)
    /// bu filtreye takılır — yani UserManager soft delete edilmiş kullanıcıyı GÖREMEZ. Admin'in silinmiş
    /// kullanıcıları listeleyebilmesi (<c>includeDeleted</c>) ve silinmiş bir kullanıcıyı kayıt olarak
    /// bulabilmesi için bu repository <c>IgnoreQueryFilters()</c> ile filtreyi bilinçli olarak bypass eder.
    /// </para>
    /// Okuma sorgularında AsNoTracking + DB seviyesinde projeksiyon; varlık kontrolleri filtresizdir.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Kullanıcıları kayıt tarihine göre (yeni → eski) sıralı, sayfalı döner. Her kullanıcının
        /// rolleri AspNetUserRoles üzerinden DB seviyesinde projekte edilir.
        /// <para>
        /// <paramref name="includeDeleted"/> true ise soft delete edilmiş kullanıcılar da dahil edilir
        /// (global query filter <c>IgnoreQueryFilters()</c> ile bypass edilir). false ise yalnızca aktif
        /// kullanıcılar döner.
        /// </para>
        /// </summary>
        Task<(IReadOnlyList<UserListItem> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            bool includeDeleted,
            CancellationToken cancellationToken);

        /// <summary>
        /// Verilen Id'ye sahip kullanıcıyı, soft delete durumundan bağımsız olarak (filtresiz),
        /// rolleriyle birlikte projekte edip döner; yoksa null. Güncelleme/oluşturma sonrası yanıt
        /// üretmek için kullanılır (yalnızca okuma; AsNoTracking).
        /// </summary>
        Task<UserListItem?> GetByIdAsync(string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Verilen e-postaya sahip kullanıcının soft delete edilmiş olup olmadığını döner.
        /// Kullanıcı yoksa false döner (varlık sızdırmamak için login akışında jenerik 401 ile birleşir).
        /// Filtresiz sorgular (silinmiş kullanıcıyı da görebilmek için).
        /// </summary>
        Task<bool> IsDeletedByEmailAsync(string email, CancellationToken cancellationToken);

        /// <summary>
        /// Verilen Id'ye sahip kullanıcının tam adını (FirstName + " " + LastName) döner; kullanıcı
        /// yoksa null. Arama logunda "kim aradı" snapshot'ını üretmek için kullanılır. Filtresiz
        /// (silinmiş kullanıcı da çözülebilir) ve yalnızca okuma (AsNoTracking + DB projeksiyon).
        /// </summary>
        Task<string?> GetFullNameByIdAsync(string userId, CancellationToken cancellationToken);
    }
}
