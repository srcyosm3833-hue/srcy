using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Zn.Application.Common.Results;
using Zn.Application.Features.Auth.Common;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Auth.Login
{
    /// <summary>
    /// <see cref="LoginCommand"/>'ı işleyen Wolverine handler'ı.
    /// E-posta ile kullanıcıyı bulur, şifreyi SignInManager ile kontrol eder
    /// (lockout açık), başarıda access + refresh token üretir.
    /// Kullanıcı yoksa da yanlış şifredeki ile AYNI jenerik 401 döner —
    /// kullanıcı varlığı ve "hangi alan hatalı" bilgisi sızdırılmaz.
    /// </summary>
    public static class LoginCommandHandler
    {
        public static async Task<Result<AuthTokensResponse>> Handle(
            LoginCommand command,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IAuthTokenFactory tokenFactory,
            CancellationToken cancellationToken)
        {
            User? user = await userManager.FindByEmailAsync(command.Email);
            if (user is null)
            {
                // Kullanıcı yok: yine de jenerik 401. (Identity hash karşılaştırması
                // yapılmadığından küçük bir timing farkı kalır; kabul edilebilir risk.)
                return Result.Failure<AuthTokensResponse>(AuthErrors.InvalidCredentials);
            }

            SignInResult signInResult = await signInManager.CheckPasswordSignInAsync(
                user, command.Password, lockoutOnFailure: true);

            if (signInResult.IsLockedOut)
            {
                return Result.Failure<AuthTokensResponse>(AuthErrors.AccountLocked);
            }

            if (!signInResult.Succeeded)
            {
                return Result.Failure<AuthTokensResponse>(AuthErrors.InvalidCredentials);
            }

            IList<string> roles = await userManager.GetRolesAsync(user);

            AuthTokensResponse tokens = await tokenFactory.IssueAsync(
                user.Id,
                user.Email!,
                user.UserName!,
                (IReadOnlyCollection<string>)roles,
                cancellationToken);

            return Result.Success(tokens);
        }
    }
}
