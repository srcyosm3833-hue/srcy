using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Zn.Application.Interfaces.Authentication;

namespace Zn.Infrastructure.Authentication
{
    /// <summary>
    /// <see cref="IJwtTokenService"/>'in HMAC-SHA256 tabanlı implementasyonu.
    /// Access token'ları imzalar ve opak refresh token string'leri üretir.
    /// Refresh token'ın saklanması/rotation'ı bu servisin sorumluluğunda değildir
    /// (bkz. Faz 1 RefreshToken entity'si ve handler'ları).
    /// </summary>
    public sealed class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _settings;

        public JwtTokenService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        /// <inheritdoc />
        public AccessToken GenerateAccessToken(TokenUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            DateTime expiresAtUtc = DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes);

            // jti: token'a benzersiz kimlik; ileride blacklist/iptal senaryolarında işe yarar.
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.UserId),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(ClaimTypes.NameIdentifier, user.UserId),
                new(ClaimTypes.Name, user.UserName)
            };

            // Her rol ayrı bir claim olarak eklenir; [Authorize(Roles = "...")] ile eşleşir.
            foreach (string role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAtUtc,
                signingCredentials: credentials);

            string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

            return new AccessToken(tokenValue, expiresAtUtc);
        }

        /// <inheritdoc />
        public string GenerateRefreshToken()
        {
            // 64 bayt kriptografik rastgelelik → tahmin edilemez, opak token.
            byte[] randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
