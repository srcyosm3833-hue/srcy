using FluentValidation;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Comments.Add
{
    /// <summary>
    /// <see cref="AddCommentCommand"/> için FluentValidation kuralları. Azami uzunluk
    /// <see cref="Comment.CommentTextMaxLength"/> sabitiyle (ve dolayısıyla CommentConfiguration'daki
    /// HasMaxLength(1000) ile) senkrondur. Validator yalnızca komuta uygulanır; entity invariant'ları
    /// Domain'dedir. Blog varlığı (DB kontrolü) handler'da yapılır.
    /// </summary>
    public sealed class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
    {
        public AddCommentCommandValidator()
        {
            RuleFor(x => x.CommentText)
                .NotEmpty().WithMessage("Comment text is required.")
                .MaximumLength(Comment.CommentTextMaxLength)
                .WithMessage($"Comment text must not exceed {Comment.CommentTextMaxLength} characters.");

            // Defansif: UserId controller'da token'dan doldurulur; yine de boş gelmediğini garanti et.
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Author could not be resolved from the access token.");
        }
    }
}
