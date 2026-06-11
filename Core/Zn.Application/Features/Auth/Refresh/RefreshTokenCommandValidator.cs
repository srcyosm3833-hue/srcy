using FluentValidation;

namespace Zn.Application.Features.Auth.Refresh
{
    /// <summary>
    /// <see cref="RefreshTokenCommand"/> için FluentValidation kuralları.
    /// Yalnızca token'ın gönderildiğini doğrular; geçerlilik/revoke kontrolü
    /// handler'da, jenerik 401 ile yapılır.
    /// </summary>
    public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required.");
        }
    }
}
