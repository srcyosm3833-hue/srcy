using System;
using FluentValidation;
using DomainSocialMedia = Zn.Domain.Entity.SocialMedia;

namespace Zn.Application.Features.SocialMedia.Update
{
    /// <summary>
    /// <see cref="UpdateSocialMediaCommand"/> için FluentValidation kuralları.
    /// Id boş olamaz; Title/Url/Icon zorunlu ve azami uzunlukları <see cref="DomainSocialMedia"/>
    /// sabitleri (dolayısıyla SocialMediaConfiguration) ile uyumludur. Url ayrıca geçerli mutlak
    /// http/https bağlantısı olmalıdır. Validator yalnızca komuta uygulanır; entity invariant'ları Domain'dedir.
    /// </summary>
    public sealed class UpdateSocialMediaCommandValidator : AbstractValidator<UpdateSocialMediaCommand>
    {
        public UpdateSocialMediaCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Social media id is required.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(DomainSocialMedia.TitleMaxLength)
                .WithMessage($"Title must not exceed {DomainSocialMedia.TitleMaxLength} characters.");

            RuleFor(x => x.Url)
                .NotEmpty().WithMessage("Url is required.")
                .MaximumLength(DomainSocialMedia.UrlMaxLength)
                .WithMessage($"Url must not exceed {DomainSocialMedia.UrlMaxLength} characters.")
                .Must(BeValidAbsoluteUrl).WithMessage("Url must be a valid absolute http or https URL.");

            RuleFor(x => x.Icon)
                .NotEmpty().WithMessage("Icon is required.")
                .MaximumLength(DomainSocialMedia.IconMaxLength)
                .WithMessage($"Icon must not exceed {DomainSocialMedia.IconMaxLength} characters.");
        }

        /// <summary>
        /// Değerin geçerli bir mutlak http/https URL olup olmadığını döner. Boş değerler
        /// burada true sayılır; zorunluluk kuralını NotEmpty ayrıca raporlar (çift hata önlenir).
        /// </summary>
        private static bool BeValidAbsoluteUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return true;
            }

            return Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
