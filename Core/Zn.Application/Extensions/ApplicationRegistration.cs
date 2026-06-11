using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zn.Application.Features.Auth.Common;

namespace Zn.Application.Extensions
{
    /// <summary>
    /// Application katmanının DI kayıtlarını tek noktada toplar. WebApi tarafında
    /// Program.cs içinde builder.Services.AddApplicationServices() çağrısı yeterlidir.
    /// <para>
    /// Wolverine kaydı bilinçli olarak burada DEĞİLDİR: Wolverine host seviyesinde
    /// (builder.Host.UseWolverine(...)) kaydedilir ve handler keşfi için bu assembly'yi
    /// taraması gerekir. Bu sınıf yalnızca FluentValidation validator'larını
    /// assembly taramasıyla kaydeder.
    /// </para>
    /// </summary>
    public static class ApplicationRegistration
    {
        /// <summary>
        /// Bu assembly'deki tüm <see cref="IValidator{T}"/> implementasyonlarını
        /// (AbstractValidator türevleri) DI'a ekler.
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            Assembly applicationAssembly = typeof(ApplicationRegistration).Assembly;

            // Tüm AbstractValidator<T> türevlerini Scoped olarak kaydeder.
            services.AddValidatorsFromAssembly(applicationAssembly, includeInternalTypes: true);

            // Refresh token süresi gibi auth ayarları "JwtSettings" bölümünden bind edilir
            // (Infrastructure'daki JwtSettings ile aynı bölüm, ayrı tip).
            services.Configure<AuthTokenOptions>(configuration.GetSection(AuthTokenOptions.SectionName));

            // Login/refresh akışlarının paylaştığı token üretim+kaydetme yardımcısı.
            services.AddScoped<IAuthTokenFactory, AuthTokenFactory>();

            return services;
        }

        /// <summary>
        /// Wolverine'in handler keşfi için taraması gereken Application assembly'si.
        /// Program.cs'te builder.Host.UseWolverine(opts =>
        /// opts.Discovery.IncludeAssembly(ApplicationRegistration.ApplicationAssembly))
        /// şeklinde kullanılır.
        /// </summary>
        public static Assembly ApplicationAssembly => typeof(ApplicationRegistration).Assembly;
    }
}
