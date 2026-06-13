using System.Linq;
using Microsoft.AspNetCore.Http;
using Zn.Application.Interfaces.Audit;

namespace Zn.ClientWebApi.Audit
{
    /// <summary>
    /// <see cref="IClientIpResolver"/>'ın ASP.NET implementasyonu. Geçerli isteğin istemci IP'sini
    /// <see cref="IHttpContextAccessor"/> üzerinden çözer.
    /// <para>
    /// Çözümleme önceliği: önce <c>X-Forwarded-For</c> başlığının ilk (en soldaki = orijinal istemci)
    /// değeri, ardından fallback olarak <c>HttpContext.Connection.RemoteIpAddress</c>. HttpContext
    /// yoksa (örn. arka plan işi, bazı test senaryoları) veya IP yoksa null döner ve ASLA hata
    /// fırlatmaz — audit opsiyoneldir ve asıl iş akışını bloklamaz.
    /// </para>
    /// <para>
    /// Güvenlik notu: <c>X-Forwarded-For</c> güvenilir bir proxy/CDN arkasında değilse istemci
    /// tarafından sahte doldurulabilir. Üretimde ters proxy için <c>ForwardedHeaders</c> middleware'i
    /// ve güvenilir proxy yapılandırması değerlendirilmelidir (R6). Bu altyapı yalnızca ham IP'yi
    /// çözer; saklama her zaman hash'li yapılır.
    /// </para>
    /// </summary>
    public sealed class ClientIpResolver : IClientIpResolver
    {
        private const string ForwardedForHeader = "X-Forwarded-For";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientIpResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        public string? ResolveIpAddress()
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return null;
            }

            // 1) X-Forwarded-For: "client, proxy1, proxy2" — ilk değer orijinal istemcidir.
            if (httpContext.Request.Headers.TryGetValue(ForwardedForHeader, out var forwardedFor))
            {
                string? firstHop = forwardedFor
                    .ToString()
                    .Split(',')
                    .Select(value => value.Trim())
                    .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

                if (!string.IsNullOrWhiteSpace(firstHop))
                {
                    return firstHop;
                }
            }

            // 2) Fallback: doğrudan bağlantının uzak IP'si.
            return httpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}
