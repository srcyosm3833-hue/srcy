using FluentValidation;

namespace Zn.Application.Features.Comments.ToggleLike
{
    /// <summary>
    /// <see cref="ToggleCommentLikeCommand"/> için FluentValidation kuralları. Validator yalnızca
    /// komuta uygulanır; şekilsel doğrulama yapar (CommentId/UserId boş olamaz). Yorumun gerçekten
    /// var olup olmadığı DB kontrolü handler'da yapılır (yoksa 404).
    /// </summary>
    public sealed class ToggleCommentLikeCommandValidator : AbstractValidator<ToggleCommentLikeCommand>
    {
        public ToggleCommentLikeCommandValidator()
        {
            RuleFor(x => x.CommentId)
                .NotEmpty().WithMessage("Comment id is required.");

            // Defansif: UserId controller'da token'dan doldurulur; yine de boş gelmediğini garanti et.
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User could not be resolved from the access token.");
        }
    }
}
