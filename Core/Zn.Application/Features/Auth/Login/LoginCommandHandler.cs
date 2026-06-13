using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Zn.Application.Common.Results;
using Zn.Application.Features.Auth.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Auth.Login
{
    /// <summary>
    /// <see cref="LoginCommand"/>'ı işleyen Wolverine handler'ı.
    /// E-posta ile kullanıcıyı bulur, şifreyi SignInManager ile kontrol eder
    /// (lockout açık), başarıda access + refresh token üretir.
    /// Kullanıcı yoksa da yanlış şifredeki ile AYNI jenerik 401 döner —
    /// kullanıcı varlığı ve "hangi alan hatalı" bilgisi sızdırılmaz.
    /// <para>
    /// Soft delete edilmiş kullanıcı login olamaz: <see cref="UserManager{TUser}.FindByEmailAsync"/>
    /// global query filter nedeniyle silinmiş kullanıcıyı null döndürür. Bu durumda filtresiz bir
    /// kontrolle (<see cref="IUserRepository.IsDeletedByEmailAsync"/>) hesabın silinmiş olup olmadığı
    /// ayırt edilir ve anlamlı "AccountDisabled" (401) döndürülür; hesap gerçekten yoksa jenerik 401 kalır.
    /// </para>
    /// </summary>
    public static class LoginCommandHandler
    {
        public static async Task<Result<AuthTokensResponse>> Handle(
            LoginCommand command,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IUserRepository userRepository,
            IAuthTokenFactory tokenFactory,
            CancellationToken cancellationToken)
        {
            User? user = await userManager.FindByEmailAsync(command.Email);
            if (user is null)
            {
                // Kullanıcı normal sorguda yok. Soft delete edilmiş olabilir (filtre gizliyor) →
                // filtresiz kontrol et: silinmişse anlamlı "AccountDisabled", değilse jenerik 401.
                bool isDeleted = await userRepository.IsDeletedByEmailAsync(command.Email, cancellationToken);
                return isDeleted
                    ? Result.Failure<AuthTokensResponse>(AuthErrors.AccountDisabled)
                    : Result.Failure<AuthTokensResponse>(AuthErrors.InvalidCredentials);
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
