using FluentValidation;

namespace Zn.Application.Features.Auth.Register
{
    /// <summary>
    /// <see cref="RegisterCommand"/> için FluentValidation kuralları.
    /// Şifre politikası Identity ayarlarıyla (min 8, büyük harf, rakam) uyumludur;
    /// böylece istemci, sunucu tarafına gitmeden net hata mesajları alır.
    /// Validator yalnızca command'a uygulanır; entity invariant'ları Domain/Identity'dedir.
    /// </summary>
    public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.")
                .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit.");

            // ImageUrl opsiyoneldir (boş/null kabul edilir). Boş gelirse handler
            // varsayılan avatar atar. Yalnızca dolu geldiğinde uzunluk sınırı uygulanır.
            RuleFor(x => x.ImageUrl)
                .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters.");
        }
    }
}
