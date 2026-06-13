using FluentValidation;

namespace Zn.Application.Features.Blogs.ToggleLike
{
    /// <summary>
    /// <see cref="ToggleBlogLikeCommand"/> için FluentValidation kuralları. Validator yalnızca
    /// komuta uygulanır; şekilsel doğrulama yapar (BlogId/UserId boş olamaz). Blogun gerçekten
    /// var olup olmadığı DB kontrolü handler'da yapılır (yoksa 404).
    /// </summary>
    public sealed class ToggleBlogLikeCommandValidator : AbstractValidator<ToggleBlogLikeCommand>
    {
        public ToggleBlogLikeCommandValidator()
        {
            RuleFor(x => x.BlogId)
                .NotEmpty().WithMessage("Blog id is required.");

            // Defansif: UserId controller'da token'dan doldurulur; yine de boş gelmediğini garanti et.
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User could not be resolved from the access token.");
        }
    }
}
