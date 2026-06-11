using System.Threading;
using System.Threading.Tasks;
using Zn.Domain.Entity;

namespace Zn.Application.Interfaces.Persistence
{
    /// <summary>
    /// RefreshToken kalıcılaştırma sözleşmesi. İmplementasyon Zn.Persistence'tadır.
    /// Application katmanı EF Core'a doğrudan bağımlı olmadan token yaşam döngüsünü
    /// (ekleme, bulma, rotation, toplu iptal) yönetebilir.
    /// </summary>
    public interface IRefreshTokenRepository
    {
        /// <summary>Yeni bir refresh token kaydını ekler (henüz kaydedilmez; bkz. <see cref="SaveChangesAsync"/>).</summary>
        Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

        /// <summary>
        /// Verilen hash'e sahip token'ı (varsa) döner. Rotation/replay kararları için
        /// revoke ve süre alanlarıyla birlikte takip edilen (tracked) entity döner.
        /// </summary>
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);

        /// <summary>
        /// Belirtilen kullanıcının tüm aktif (revoke edilmemiş ve süresi dolmamış)
        /// token'larını revoke eder (RevokedAt = şimdi). Replay saldırısı tespitinde
        /// kullanılır. Değişiklikler henüz kaydedilmez; bkz. <see cref="SaveChangesAsync"/>.
        /// </summary>
        Task RevokeAllActiveForUserAsync(string userId, CancellationToken cancellationToken);

        /// <summary>Bekleyen değişiklikleri veritabanına yazar.</summary>
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
