using Microsoft.Extensions.DependencyInjection;

namespace Zn.ClientWebApi.Extensions
{
    /// <summary>
    /// CORS politikalarını kurar. Development için frontend origin'lerine
    /// (Vite varsayılanı dahil) izin verilir. Production origin'leri ileride
    /// yapılandırmadan okunacak şekilde genişletilebilir.
    /// </summary>
    public static class CorsRegistration
    {
        /// <summary>Program.cs'te UseCors ile başvurulan politika adı.</summary>
        public const string DevelopmentPolicy = "ZnDevelopmentCors";

        public static IServiceCollection AddCorsPolicies(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(DevelopmentPolicy, policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:5173",  // Vite dev server (HTTP)
                            "https://localhost:5173", // Vite dev server (HTTPS)
                            "http://localhost:3000")  // alternatif frontend portu
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            return services;
        }
    }
}
