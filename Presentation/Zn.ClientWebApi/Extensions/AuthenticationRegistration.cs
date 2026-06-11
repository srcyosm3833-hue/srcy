using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Zn.Infrastructure.Authentication;

namespace Zn.ClientWebApi.Extensions
{
    /// <summary>
    /// WebApi tarafında JWT Bearer authentication şemasını kurar.
    /// TokenValidationParameters, "JwtSettings" bölümünden okunur ve
    /// JwtTokenService'in ürettiği token'larla birebir uyumlu olacak şekilde
    /// (issuer, audience, imza anahtarı, süre) yapılandırılır.
    /// </summary>
    public static class AuthenticationRegistration
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            JwtSettings settings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                ?? new JwtSettings();

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey));

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = settings.Issuer,

                        ValidateAudience = true,
                        ValidAudience = settings.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,

                        ValidateLifetime = true,
                        // Token süre kontrolünde saat kaymasını sıfırla; ~15 dk'lık
                        // access token'da varsayılan 5 dk tolerans yanıltıcı olur.
                        ClockSkew = System.TimeSpan.Zero
                    };
                });

            return services;
        }
    }
}
