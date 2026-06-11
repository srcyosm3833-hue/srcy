using FluentValidation;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Comments.Update
{
    /// <summary>
    /// <see cref="UpdateCommentCommand"/> için FluentValidation kuralları. Azami uzunluk
    /// <see cref="Comment.CommentTextMaxLength"/> sabitiyle senkrondur. Validator yalnızca komuta
    /// uygulanır; entity invariant'ları Domain'dedir. Sahiplik kontrolü handler'da yapılır.
    /// </summary>
    public sealed class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
    {
        public UpdateCommentCommandValidator()
        {
            RuleFor(x => x.CommentText)
                .NotEmpty().WithMessage("Comment text is required.")
                .MaximumLength(Comment.CommentTextMaxLength)
                .WithMessage($"Comment text must not exceed {Comment.CommentTextMaxLength} characters.");

            RuleFor(x => x.RequestingUserId)
                .NotEmpty().WithMessage("Requesting user could not be resolved from the access token.");
        }
    }
}
