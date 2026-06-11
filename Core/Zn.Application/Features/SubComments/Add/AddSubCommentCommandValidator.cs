using FluentValidation;
using Zn.Domain.Entity;

namespace Zn.Application.Features.SubComments.Add
{
    /// <summary>
    /// <see cref="AddSubCommentCommand"/> için FluentValidation kuralları. Azami uzunluk
    /// <see cref="SubComment.SubCommentTextMaxLength"/> sabitiyle (ve dolayısıyla
    /// SubCommentConfiguration'daki HasMaxLength(1000) ile) senkrondur. Validator yalnızca komuta
    /// uygulanır; entity invariant'ları Domain'dedir. Ana yorum varlığı (DB kontrolü) handler'da yapılır.
    /// </summary>
    public sealed class AddSubCommentCommandValidator : AbstractValidator<AddSubCommentCommand>
    {
        public AddSubCommentCommandValidator()
        {
            RuleFor(x => x.SubCommentText)
                .NotEmpty().WithMessage("Sub-comment text is required.")
                .MaximumLength(SubComment.SubCommentTextMaxLength)
                .WithMessage($"Sub-comment text must not exceed {SubComment.SubCommentTextMaxLength} characters.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Author could not be resolved from the access token.");
        }
    }
}
