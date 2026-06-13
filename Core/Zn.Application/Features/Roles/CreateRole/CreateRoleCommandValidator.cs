using FluentValidation;

namespace Zn.Application.Features.Roles.CreateRole
{
    /// <summary>
    /// <see cref="CreateRoleCommand"/> için FluentValidation kuralları. Rol adı zorunlu ve uzunluk
    /// sınırlıdır (Identity Name kolonu varsayılan 256). Validator yalnızca komuta uygulanır;
    /// benzersizlik ve korumalı rol kontrolü handler'da iş kuralı olarak yapılır.
    /// </summary>
    public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required.")
                .MaximumLength(256).WithMessage("Role name must not exceed 256 characters.");
        }
    }
}
