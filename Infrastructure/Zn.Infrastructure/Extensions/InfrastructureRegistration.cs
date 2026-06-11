using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zn.Application.Interfaces.Authentication;
using Zn.Infrastructure.Authentication;

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

            return services;
        }
    }
}
