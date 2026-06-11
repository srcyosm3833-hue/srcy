using FluentValidation;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Blogs.Update
{
    /// <summary>
    /// <see cref="UpdateBlogCommand"/> için FluentValidation kuralları. Azami uzunluklar
    /// <see cref="Blog"/> sabitleriyle senkrondur. Validator yalnızca şekilsel doğrulama yapar;
    /// kategori varlığı, kayıt varlığı ve yetki kontrolü handler'da gerçekleşir.
    /// </summary>
    public sealed class UpdateBlogCommandValidator : AbstractValidator<UpdateBlogCommand>
    {
        public UpdateBlogCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Blog id is required.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(Blog.TitleMaxLength)
                .WithMessage($"Title must not exceed {Blog.TitleMaxLength} characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.");

            RuleFor(x => x.CoverImage)
                .NotEmpty().WithMessage("Cover image is required.")
                .MaximumLength(Blog.ImageUrlMaxLength)
                .WithMessage($"Cover image must not exceed {Blog.ImageUrlMaxLength} characters.");

            RuleFor(x => x.BlogImage)
                .NotEmpty().WithMessage("Blog image is required.")
                .MaximumLength(Blog.ImageUrlMaxLength)
                .WithMessage($"Blog image must not exceed {Blog.ImageUrlMaxLength} characters.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category is required.");

            // Defansif: kimlik controller'da token'dan doldurulur.
            RuleFor(x => x.RequestingUserId)
                .NotEmpty().WithMessage("Requesting user could not be resolved from the access token.");
        }
    }
}
