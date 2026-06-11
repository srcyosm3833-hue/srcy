using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Zn.Persistence.Context;

namespace Zn.ClientWebApi.IntegrationTests.Infrastructure
{
    /// <summary>
    /// LocalDB üzerinde test'e özel izole veritabanı kullanan WebApplicationFactory.
    /// Docker/TestContainers kullanılmaz; her test oturumu kendine özgü bir
    /// "ZnApp_Test_{runId}" veritabanı oluşturur ve oturum sonunda siler.
    ///
    /// DB izolasyonu: her AuthApiFixture örneği farklı bir DB adı alır →
    /// paralel çalışan test oturumları birbirini etkilemez.
    /// Tek oturum içindeki testler aynı DB paylaşır; her test kendi benzersiz
    /// kullanıcısını farklı e-posta ile oluşturur → test-seviyesi izolasyon sağlanır.
    /// </summary>
    public sealed class AuthWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        // Her factory örneği için benzersiz DB adı: paralel çalışmaya dayanıklı.
        private readonly string _databaseName =
            "ZnApp_Test_" + Guid.NewGuid().ToString("N");

        private const string LocalDbServer = @"(localdb)\MSSQLLocalDB";

        public string ConnectionString =>
            $"Server={LocalDbServer};Database={_databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";

        public async Task InitializeAsync()
        {
            using IServiceScope scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ZnBlogDbContext>();
            // EnsureDeleted → temiz slate; ardından Migrate → şema + seed.
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        public new async Task DisposeAsync()
        {
            // Test oturumu bitti: izole DB'yi tamamen sil.
            using IServiceScope scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ZnBlogDbContext>();
            await db.Database.EnsureDeletedAsync();
            await base.DisposeAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureServices(services =>
            {
                // Üretim DbContext kaydını kaldır; LocalDB test DB'siyle değiştir.
                services.RemoveAll<DbContextOptions<ZnBlogDbContext>>();
                services.RemoveAll<ZnBlogDbContext>();

                services.AddDbContext<ZnBlogDbContext>(options =>
                    options.UseSqlServer(ConnectionString));
            });
        }
    }
}
