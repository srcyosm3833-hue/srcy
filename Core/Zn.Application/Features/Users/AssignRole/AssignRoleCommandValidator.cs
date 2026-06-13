using FluentValidation;

namespace Zn.Application.Features.Users.AssignRole
{
    /// <summary>
    /// <see cref="AssignRoleCommand"/> için FluentValidation kuralları. UserId ve RoleName zorunludur.
    /// Kullanıcı/rol varlığı handler'da iş kuralı olarak (404) doğrulanır.
    /// </summary>
    public sealed class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
    {
        public AssignRoleCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User id is required.");

            RuleFor(x => x.RoleName)
                .NotEmpty().WithMessage("Role name is required.");
        }
    }
}
