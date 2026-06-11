using FluentValidation;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Blogs.Create
{
    /// <summary>
    /// <see cref="CreateBlogCommand"/> için FluentValidation kuralları. Azami uzunluklar
    /// <see cref="Blog"/> sabitleriyle (ve dolayısıyla BlogConfiguration'daki HasMaxLength'lerle)
    /// senkrondur. Validator yalnızca komuta uygulanır; entity invariant'ları Domain'dedir.
    /// Kategori varlığı (DB kontrolü) handler'da yapılır — validator yalnızca şekilsel doğrular.
    /// </summary>
    public sealed class CreateBlogCommandValidator : AbstractValidator<CreateBlogCommand>
    {
        public CreateBlogCommandValidator()
        {
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

            // Defansif: UserId controller'da token'dan doldurulur; yine de boş gelmediğini garanti et.
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Author could not be resolved from the access token.");
        }
    }
}
