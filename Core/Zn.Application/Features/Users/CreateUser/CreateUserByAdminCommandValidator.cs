using FluentValidation;

namespace Zn.Application.Features.Users.CreateUser
{
    /// <summary>
    /// <see cref="CreateUserByAdminCommand"/> için FluentValidation kuralları. Şifre politikası
    /// Identity ayarlarıyla (min 8, büyük/küçük harf, rakam) uyumludur. Validator yalnızca komuta
    /// uygulanır; entity invariant'ları Identity'dedir. RegisterCommandValidator ile aynı kuralları izler.
    /// </summary>
    public sealed class CreateUserByAdminCommandValidator : AbstractValidator<CreateUserByAdminCommand>
    {
        public CreateUserByAdminCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

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

            // ImageUrl opsiyoneldir (boş/null kabul edilir). Yalnızca dolu geldiğinde uzunluk sınırı.
            RuleFor(x => x.ImageUrl)
                .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters.");
        }
    }
}
