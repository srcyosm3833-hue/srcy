using FluentValidation;

namespace Zn.Application.Features.Auth.Logout
{
    /// <summary>
    /// <see cref="LogoutCommand"/> için FluentValidation kuralları.
    /// Yalnızca token'ın gönderildiğini doğrular.
    /// </summary>
    public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
    {
        public LogoutCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required.");
        }
    }
}
