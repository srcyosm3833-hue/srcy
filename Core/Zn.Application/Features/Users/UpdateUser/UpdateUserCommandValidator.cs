using FluentValidation;

namespace Zn.Application.Features.Users.UpdateUser
{
    /// <summary>
    /// <see cref="UpdateUserCommand"/> için FluentValidation kuralları. Yalnızca komuta uygulanır;
    /// entity invariant'ları (NOT NULL alanlar) Persistence konfigürasyonunda korunur.
    /// ImageUrl opsiyoneldir (boşsa handler varsayılan avatarı uygular), bu yüzden burada doğrulanmaz.
    /// </summary>
    public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        /// <summary>Ad/soyad için makul azami uzunluk (UI ve veri bütünlüğü sınırı).</summary>
        private const int NameMaxLength = 100;

        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User id is required.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(NameMaxLength)
                .WithMessage($"First name must not exceed {NameMaxLength} characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(NameMaxLength)
                .WithMessage($"Last name must not exceed {NameMaxLength} characters.");
        }
    }
}
