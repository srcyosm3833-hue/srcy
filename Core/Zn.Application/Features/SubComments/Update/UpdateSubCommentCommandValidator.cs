using FluentValidation;
using Zn.Domain.Entity;

namespace Zn.Application.Features.SubComments.Update
{
    /// <summary>
    /// <see cref="UpdateSubCommentCommand"/> için FluentValidation kuralları. Azami uzunluk
    /// <see cref="SubComment.SubCommentTextMaxLength"/> sabitiyle senkrondur. Validator yalnızca
    /// komuta uygulanır; entity invariant'ları Domain'dedir. Sahiplik kontrolü handler'da yapılır.
    /// </summary>
    public sealed class UpdateSubCommentCommandValidator : AbstractValidator<UpdateSubCommentCommand>
    {
        public UpdateSubCommentCommandValidator()
        {
            RuleFor(x => x.SubCommentText)
                .NotEmpty().WithMessage("Sub-comment text is required.")
                .MaximumLength(SubComment.SubCommentTextMaxLength)
                .WithMessage($"Sub-comment text must not exceed {SubComment.SubCommentTextMaxLength} characters.");

            RuleFor(x => x.RequestingUserId)
                .NotEmpty().WithMessage("Requesting user could not be resolved from the access token.");
        }
    }
}
