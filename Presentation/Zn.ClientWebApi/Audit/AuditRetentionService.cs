using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zn.Persistence.Context;

namespace Zn.ClientWebApi.Audit
{
    /// <summary>
    /// Audit saklama süresi (retention) temizliğini periyodik olarak yürüten arka plan servisi
    /// (KVKK m.4/2-e — gerektiğinden uzun süre saklamama). <see cref="AuditRetentionOptions"/>'a göre:
    /// <list type="bullet">
    /// <item>Blog <c>CreatorIpHash</c> ve Message <c>SenderIpHash</c>: süresi geçen kayıtlarda IP hash'i
    /// null'a çekilir (anonimleştirme — kayıt korunur).</item>
    /// <item>SearchLog: süresi geçen satırlar tamamen silinir.</item>
    /// </list>
    /// <para>
    /// Bir tür için süre 0/negatifse o temizlik atlanır. Her tarama izole try/catch içindedir:
    /// bir hata (örn. geçici DB kesintisi) uygulamayı ÇÖKERTMEZ, yalnızca yapılandırılmış log bırakır
    /// ve bir sonraki turda yeniden denenir. Toplu işlemler <c>ExecuteUpdateAsync</c>/<c>ExecuteDeleteAsync</c>
    /// ile DB tarafında yapılır (entity belleğe çekilmez). Global query filter'lar bypass edilir ki
    /// soft-delete edilmiş kayıtların da IP hash'i süresinde anonimleşsin.
    /// </para>
    /// </summary>
    public sealed class AuditRetentionService : BackgroundService
    {
        private const int FallbackSweepIntervalHours = 24;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptionsMonitor<AuditRetentionOptions> _optionsMonitor;
        private readonly ILogger<AuditRetentionService> _logger;

        public AuditRetentionService(
            IServiceScopeFactory scopeFactory,
            IOptionsMonitor<AuditRetentionOptions> optionsMonitor,
            ILogger<AuditRetentionService> logger)
        {
            _scopeFactory = scopeFactory;
            _optionsMonitor = optionsMonitor;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                AuditRetentionOptions options = _optionsMonitor.CurrentValue;

                // Tek bir tarama turu; tüm hatalar içeride yutulur (app çökmesin).
                await RunSweepAsync(options, stoppingToken);

                // Aralık 0/negatifse güvenli varsayılana düş; asla sıfır/negatif gecikme verme.
                int intervalHours = options.SweepIntervalHours > 0
                    ? options.SweepIntervalHours
                    : FallbackSweepIntervalHours;

                try
                {
                    await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Uygulama kapanıyor; sessizce çık.
                    break;
                }
            }
        }

        /// <summary>
        /// Tek bir temizlik turu çalıştırır. Her tür kendi izole try/catch'i içindedir; biri patlarsa
        /// diğerleri yine de denenir ve servis döngüsü devam eder.
        /// </summary>
        private async Task RunSweepAsync(AuditRetentionOptions options, CancellationToken cancellationToken)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            ZnBlogDbContext dbContext = scope.ServiceProvider.GetRequiredService<ZnBlogDbContext>();
            DateTime nowUtc = DateTime.UtcNow;

            // 1) Blog IP hash anonimleştirme.
            if (options.BlogIpDays > 0)
            {
                DateTime cutoff = nowUtc.AddDays(-options.BlogIpDays);
                try
                {
                    int affected = await dbContext.Blogs
                        .IgnoreQueryFilters()
                        .Where(b => b.CreatorIpHash != null && b.CreatedAt < cutoff)
                        .ExecuteUpdateAsync(
                            setters => setters.SetProperty(b => b.CreatorIpHash, (string?)null),
                            cancellationToken);

                    if (affected > 0)
                    {
                        _logger.LogInformation(
                            "Audit retention: anonymized {Count} blog IP hash(es) older than {Days} days.",
                            affected, options.BlogIpDays);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Audit retention: blog IP anonymization failed; will retry next sweep.");
                }
            }

            // 2) Message IP hash anonimleştirme.
            if (options.MessageIpDays > 0)
            {
                DateTime cutoff = nowUtc.AddDays(-options.MessageIpDays);
                try
                {
                    int affected = await dbContext.Messages
                        .IgnoreQueryFilters()
                        .Where(m => m.SenderIpHash != null && m.CreatedAt < cutoff)
                        .ExecuteUpdateAsync(
                            setters => setters.SetProperty(m => m.SenderIpHash, (string?)null),
                            cancellationToken);

                    if (affected > 0)
                    {
                        _logger.LogInformation(
                            "Audit retention: anonymized {Count} message IP hash(es) older than {Days} days.",
                            affected, options.MessageIpDays);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Audit retention: message IP anonymization failed; will retry next sweep.");
                }
            }

            // 3) SearchLog satır silme (anonimleştirme değil — tamamen kaldırılır).
            if (options.SearchLogDays > 0)
            {
                DateTime cutoff = nowUtc.AddDays(-options.SearchLogDays);
                try
                {
                    int affected = await dbContext.SearchLogs
                        .Where(s => s.SearchedAt < cutoff)
                        .ExecuteDeleteAsync(cancellationToken);

                    if (affected > 0)
                    {
                        _logger.LogInformation(
                            "Audit retention: deleted {Count} search log(s) older than {Days} days.",
                            affected, options.SearchLogDays);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Audit retention: search log deletion failed; will retry next sweep.");
                }
            }
        }
    }
}
