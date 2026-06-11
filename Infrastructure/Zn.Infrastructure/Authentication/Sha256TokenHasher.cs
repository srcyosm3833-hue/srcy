using System.Security.Cryptography;
using System.Text;
using Zn.Application.Interfaces.Authentication;

namespace Zn.Infrastructure.Authentication
{
    /// <summary>
    /// <see cref="ITokenHasher"/>'ın deterministik SHA-256 implementasyonu.
    /// Refresh token'lar zaten 64 bayt kriptografik rastgelelik taşıdığından,
    /// parola gibi salt + yavaş hash gerekmez. Deterministik hash, DB'de unique
    /// index üzerinden birebir arama yapılabilmesini sağlar.
    /// </summary>
    public sealed class Sha256TokenHasher : ITokenHasher
    {
        /// <inheritdoc />
        public string Hash(string token)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(token);
            byte[] hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
