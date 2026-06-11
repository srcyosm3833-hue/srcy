using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Zn.Application.Common.Results;
using Zn.Application.Features.Auth.Common;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Auth.Register
{
    /// <summary>
    /// <see cref="RegisterCommand"/>'ı işleyen Wolverine handler'ı (plain metot konvansiyonu).
    /// Kullanıcıyı UserManager üzerinden oluşturur; başarıda Id + e-posta döner.
    /// İş mantığı incedir: doğrulama validator'da, şifre politikası Identity'dedir.
    /// </summary>
    public static class RegisterCommandHandler
    {
        public static async Task<Result<RegisterResponse>> Handle(
            RegisterCommand command,
            UserManager<User> userManager,
            CancellationToken cancellationToken)
        {
            // Duplicate e-posta erken kontrolü: anlamlı 409 döndürmek için.
            User? existing = await userManager.FindByEmailAsync(command.Email);
            if (existing is not null)
            {
                return Result.Failure<RegisterResponse>(AuthErrors.EmailAlreadyExists(command.Email));
            }

            var user = new User
            {
                UserName = command.Email,
                Email = command.Email,
                FirstName = command.FirstName,
                LastName = command.LastName,
                ImageUrl = command.ImageUrl
            };

            IdentityResult result = await userManager.CreateAsync(user, command.Password);
            if (!result.Succeeded)
            {
                // Identity hatalarını (örn. DuplicateUserName, PasswordTooShort) yarış
                // koşulu ya da politika ihlali olarak alan bazlı validation hatasına eşle.
                if (result.Errors.Any(e => e.Code.Contains("Duplicate")))
                {
                    return Result.Failure<RegisterResponse>(AuthErrors.EmailAlreadyExists(command.Email));
                }

                IReadOnlyDictionary<string, string[]> validations = MapIdentityErrors(result.Errors);
                return Result.Failure<RegisterResponse>(AuthErrors.IdentityFailure(validations));
            }

            return Result.Success(new RegisterResponse(user.Id, user.Email!));
        }

        /// <summary>
        /// Identity hatalarını ProblemDetails "errors" sözlüğüne uygun
        /// alan→mesajlar yapısına çevirir. Şifre kuralı ihlalleri "Password" altında gruplanır.
        /// </summary>
        private static IReadOnlyDictionary<string, string[]> MapIdentityErrors(
            IEnumerable<IdentityError> errors)
        {
            return errors
                .GroupBy(e => e.Code.Contains("Password") ? "Password" : "Registration")
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(e => e.Description).ToArray());
        }
    }
}
