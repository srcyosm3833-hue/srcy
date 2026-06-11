using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Zn.Application.Features.Auth.Common
{
    /// <summary>
    /// Bir kullanıcı için access + refresh token çiftini üretip refresh token'ı
    /// (hash'lenmiş halde) kalıcılaştıran yardımcı sözleşme. Login ve refresh
    /// handler'ları aynı üretim/kaydetme mantığını paylaşır.
    /// </summary>
    public interface IAuthTokenFactory
    {
        /// <summary>
        /// Verilen kullanıcı için yeni bir access token ve yeni bir refresh token üretir;
        /// refresh token'ın hash'ini veritabanına kaydeder ve değişiklikleri yazar.
        /// </summary>
        /// <param name="userId">Kullanıcının Id'si.</param>
        /// <param name="email">Kullanıcının e-postası (claim'e gömülür).</param>
        /// <param name="userName">Kullanıcı adı (claim'e gömülür).</param>
        /// <param name="roles">Kullanıcının rolleri (her biri ayrı claim).</param>
        /// <param name="cancellationToken">İptal jetonu.</param>
        /// <returns>İstemciye dönecek access + düz refresh token yanıtı.</returns>
        Task<AuthTokensResponse> IssueAsync(
            string userId,
            string email,
            string userName,
            IReadOnlyCollection<string> roles,
            CancellationToken cancellationToken);
    }
}
