using FluentValidation;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Categories.Create
{
    /// <summary>
    /// <see cref="CreateCategoryCommand"/> için FluentValidation kuralları.
    /// Azami uzunluk, <see cref="Category.CategoryNameMaxLength"/> (ve dolayısıyla
    /// CategoryConfiguration'daki HasMaxLength) ile birebir aynıdır.
    /// Validator yalnızca komuta uygulanır; entity invariant'ları Domain'dedir.
    /// </summary>
    public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            RuleFor(x => x.CategoryName)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(Category.CategoryNameMaxLength)
                .WithMessage($"Category name must not exceed {Category.CategoryNameMaxLength} characters.");
        }
    }
}
