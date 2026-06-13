using FluentValidation;

namespace Zn.Application.Features.Roles.UpdateRole
{
    /// <summary>
    /// <see cref="UpdateRoleCommand"/> için FluentValidation kuralları. RoleId ve yeni ad zorunludur.
    /// Korumalı rol kontrolü, varlık ve benzersizlik handler'da iş kuralı olarak yapılır.
    /// </summary>
    public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
    {
        public UpdateRoleCommandValidator()
        {
            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("Role id is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required.")
                .MaximumLength(256).WithMessage("Role name must not exceed 256 characters.");
        }
    }
}
