using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Zn.Application.Interfaces.Authentication;

namespace Zn.Infrastructure.Authentication
{
    /// <summary>
    /// <see cref="IIpHasher"/>'ın tuzlu, deterministik SHA-256 implementasyonu. İstemci IP'sini
    /// veritabanında saklamadan önce hash'ler; ham IP ASLA tutulmaz (KVKK).
    /// <para>
    /// <b>Tuz kaynağı:</b> <c>Audit:IpHashSalt</c> yapılandırması (<see cref="IConfiguration"/>).
    /// Tuz, refresh token hash'lemesinden (<see cref="Sha256TokenHasher"/>) ayrıdır ve aynı IP'nin
    /// tahmin edilerek (rainbow table) geri çözülmesini zorlaştırır. <b>Geliştirme/test</b> için
    /// appsettings.json'da sabit bir varsayılan tuz bulunur; <b>production</b>'da gerçek tuz
    /// user-secrets / environment üzerinden sağlanmalıdır (appsettings'e gerçek değer yazılmaz).
    /// Tuz yapılandırılmamışsa güvenli bir sabit varsayılana düşülür ki hash'leme hiçbir zaman
    /// patlamasın (audit asıl iş akışını bloklamaz).
    /// </para>
    /// <para>
    /// Deterministiktir: aynı (IP, tuz) çifti her zaman aynı çıktıyı verir; böylece aynı IP'nin
    /// tekrar görünüp görünmediği (kötüye kullanım tespiti) DB tarafında eşitlikle saptanabilir.
    /// </para>
    /// </summary>
    public sealed class Sha256IpHasher : IIpHasher
    {
        /// <summary>Tuz yapılandırılmamışsa kullanılan güvenli sabit varsayılan (yalnızca acil durum).</summary>
        private const string FallbackSalt = "zn-audit-ip-fallback-salt";

        private readonly string _salt;

        public Sha256IpHasher(IConfiguration configuration)
        {
            string? configuredSalt = configuration["Audit:IpHashSalt"];
            _salt = string.IsNullOrWhiteSpace(configuredSalt) ? FallbackSalt : configuredSalt;
        }

        /// <inheritdoc />
        public string? Hash(string? ipAddress)
        {
            // Audit opsiyoneldir: IP yoksa null döner, hata fırlatmaz.
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return null;
            }

            // Tuz, IP'nin önüne eklenir; ardından tek geçişte SHA-256 alınır.
            byte[] bytes = Encoding.UTF8.GetBytes(_salt + ipAddress);
            byte[] hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
