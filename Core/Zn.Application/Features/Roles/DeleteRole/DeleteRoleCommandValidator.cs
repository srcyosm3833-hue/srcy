using FluentValidation;

namespace Zn.Application.Features.Roles.DeleteRole
{
    /// <summary>
    /// <see cref="DeleteRoleCommand"/> için FluentValidation kuralları. RoleId zorunludur. Korumalı rol,
    /// varlık ve kullanıcı atama kontrolleri handler'da iş kuralı olarak yapılır.
    /// </summary>
    public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
    {
        public DeleteRoleCommandValidator()
        {
            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("Role id is required.");
        }
    }
}
