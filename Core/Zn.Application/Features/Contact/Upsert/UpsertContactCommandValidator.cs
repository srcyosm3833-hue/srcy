using FluentValidation;
using DomainContact = Zn.Domain.Entity.Contact;

namespace Zn.Application.Features.Contact.Upsert
{
    /// <summary>
    /// <see cref="UpsertContactCommand"/> için FluentValidation kuralları. Uzunluk sınırları
    /// Contact entity'sindeki sabitlerle (ve dolayısıyla ContactConfiguration kolon limitleriyle)
    /// senkrondur. Validator yalnızca komuta uygulanır; entity invariant'ları Domain'dedir.
    /// </summary>
    public sealed class UpsertContactCommandValidator : AbstractValidator<UpsertContactCommand>
    {
        public UpsertContactCommandValidator()
        {
            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.")
                .MaximumLength(DomainContact.AddressMaxLength)
                .WithMessage($"Address must not exceed {DomainContact.AddressMaxLength} characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.")
                .MaximumLength(DomainContact.EmailMaxLength)
                .WithMessage($"Email must not exceed {DomainContact.EmailMaxLength} characters.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required.")
                .MaximumLength(DomainContact.PhoneMaxLength)
                .WithMessage($"Phone must not exceed {DomainContact.PhoneMaxLength} characters.");

            RuleFor(x => x.MapUrl)
                .NotEmpty().WithMessage("Map URL is required.")
                .MaximumLength(DomainContact.MapUrlMaxLength)
                .WithMessage($"Map URL must not exceed {DomainContact.MapUrlMaxLength} characters.");
        }
    }
}
