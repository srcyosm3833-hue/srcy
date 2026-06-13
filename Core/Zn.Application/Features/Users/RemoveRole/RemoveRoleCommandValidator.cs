using FluentValidation;

namespace Zn.Application.Features.Users.RemoveRole
{
    /// <summary>
    /// <see cref="RemoveRoleCommand"/> için FluentValidation kuralları. UserId ve RoleName zorunludur.
    /// Kullanıcı/rol varlığı ve son Admin koruması handler'da iş kuralı olarak doğrulanır.
    /// </summary>
    public sealed class RemoveRoleCommandValidator : AbstractValidator<RemoveRoleCommand>
    {
        public RemoveRoleCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User id is required.");

            RuleFor(x => x.RoleName)
                .NotEmpty().WithMessage("Role name is required.");
        }
    }
}
