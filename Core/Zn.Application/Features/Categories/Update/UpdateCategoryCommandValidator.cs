using FluentValidation;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Categories.Update
{
    /// <summary>
    /// <see cref="UpdateCategoryCommand"/> için FluentValidation kuralları.
    /// Id boş olamaz; ad zorunlu ve azami uzunluk <see cref="Category.CategoryNameMaxLength"/>
    /// ile (dolayısıyla CategoryConfiguration ile) uyumludur.
    /// Validator yalnızca komuta uygulanır; entity invariant'ları Domain'dedir.
    /// </summary>
    public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Category id is required.");

            RuleFor(x => x.CategoryName)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(Category.CategoryNameMaxLength)
                .WithMessage($"Category name must not exceed {Category.CategoryNameMaxLength} characters.");
        }
    }
}
