using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zn.Application.Interfaces.Authentication;
using Zn.Application.Interfaces.Storage;
using Zn.Infrastructure.Authentication;
using Zn.Infrastructure.Storage;

namespace Zn.Infrastructure.Extensions
{
    /// <summary>
    /// Infrastructure katmanının DI kayıtlarını tek noktada toplar. WebApi tarafında
    /// Program.cs içinde builder.Services.AddInfrastructureServices(builder.Configuration)
    /// çağrısı yeterlidir; dış servis detayları dışarı sızmaz.
    /// <para>
    /// JWT Bearer authentication şemasının kaydı (AddAuthentication / AddJwtBearer)
    /// bilinçli olarak burada DEĞİLDİR; o, TokenValidationParameters'ı kurmak için
    /// Program.cs'te yapılır. Burada yalnızca JwtSettings binding'i ve token üretim
    /// servisi kaydedilir.
    /// </para>
    /// </summary>
    public static class InfrastructureRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // JwtSettings'i "JwtSettings" bölümünden bağlar ve IOptions<JwtSettings> olarak sunar.
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

            // Token üretim servisi: stateless olduğu için Singleton uygundur.
            services.AddSingleton<IJwtTokenService, JwtTokenService>();

            // Refresh token hash'leme servisi: stateless, deterministik SHA-256.
            services.AddSingleton<ITokenHasher, Sha256TokenHasher>();

            // İstemci IP hash'leme servisi (audit): tuzlu, deterministik SHA-256. Tuz
            // "Audit:IpHashSalt" yapılandırmasından okunur; stateless olduğu için Singleton uygundur.
            services.AddSingleton<IIpHasher, Sha256IpHasher>();

            // Dosya depolama ayarları: "FileStorage" bölümünden bind edilir. RootPath fiziksel
            // yolu Program.cs'te WebRootPath'e göre doldurulur (Infrastructure ASP.NET hosting
            // tiplerini bilmez). Geliştirme için yerel disk; production'da bulut storage ile değişir.
            services.Configure<FileStorageOptions>(
                configuration.GetSection(FileStorageOptions.SectionName));

            // Yerel dosya depolama implementasyonu. Stateless olduğu için Singleton uygundur.
            services.AddSingleton<IFileStorageService, LocalFileStorageService>();

            return services;
        }
    }
}
