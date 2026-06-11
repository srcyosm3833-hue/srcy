using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Zn.Application.Interfaces.Authentication;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Auth.Common
{
    /// <summary>
    /// <see cref="IAuthTokenFactory"/>'nin varsayılan implementasyonu.
    /// Access token üretimini <see cref="IJwtTokenService"/>'e, refresh token
    /// kalıcılaştırmayı <see cref="IRefreshTokenRepository"/>'ye devreder.
    /// Refresh token DB'ye yalnızca hash'lenmiş olarak yazılır.
    /// </summary>
    public sealed class AuthTokenFactory : IAuthTokenFactory
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ITokenHasher _tokenHasher;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly AuthTokenOptions _options;

        public AuthTokenFactory(
            IJwtTokenService jwtTokenService,
            ITokenHasher tokenHasher,
            IRefreshTokenRepository refreshTokenRepository,
            IOptions<AuthTokenOptions> options)
        {
            _jwtTokenService = jwtTokenService;
            _tokenHasher = tokenHasher;
            _refreshTokenRepository = refreshTokenRepository;
            _options = options.Value;
        }

        /// <inheritdoc />
        public async Task<AuthTokensResponse> IssueAsync(
            string userId,
            string email,
            string userName,
            IReadOnlyCollection<string> roles,
            CancellationToken cancellationToken)
        {
            AccessToken accessToken = _jwtTokenService.GenerateAccessToken(
                new TokenUser(userId, email, userName, roles));

            string plainRefreshToken = _jwtTokenService.GenerateRefreshToken();
            DateTime refreshExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenDays);

            var refreshToken = new RefreshToken
            {
                Token = _tokenHasher.Hash(plainRefreshToken),
                UserId = userId,
                ExpiresAt = refreshExpiresAt,
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            return new AuthTokensResponse(
                accessToken.Value,
                accessToken.ExpiresAtUtc,
                plainRefreshToken,
                refreshExpiresAt);
        }
    }
}
