using FluentValidation;

namespace Zn.Application.Features.Auth.Login
{
    /// <summary>
    /// <see cref="LoginCommand"/> için FluentValidation kuralları.
    /// Yalnızca alanların var/biçim kontrolünü yapar; kimlik bilgisi doğruluğu
    /// (kullanıcı var mı, şifre doğru mu) handler'da, jenerik 401 ile ele alınır —
    /// kullanıcı varlığı sızdırılmaz.
    /// </summary>
    public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
