using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Categories.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Categories.Update
{
    /// <summary>
    /// <see cref="UpdateCategoryCommand"/>'ı işleyen Wolverine handler'ı.
    /// Kategori yoksa NotFound (404); yeni ad başka bir kategoride kullanılıyorsa
    /// Conflict (409). Ad değişikliği <see cref="Category.Rename"/> ile invariant
    /// korunarak yapılır; UpdatedAt orada güncellenir.
    /// </summary>
    public static class UpdateCategoryCommandHandler
    {
        public static async Task<Result<CategoryResponse>> Handle(
            UpdateCategoryCommand command,
            ICategoryRepository categoryRepository,
            CancellationToken cancellationToken)
        {
            Category? category = await categoryRepository.GetByIdAsync(command.Id, cancellationToken);
            if (category is null)
            {
                return Result.Failure<CategoryResponse>(CategoryErrors.NotFound(command.Id));
            }

            string newName = command.CategoryName.Trim();

            // Kendisi hariç aynı ada sahip başka kategori varsa çakışma.
            bool nameTaken = await categoryRepository.ExistsByNameAsync(
                newName, excludeId: command.Id, cancellationToken);

            if (nameTaken)
            {
                return Result.Failure<CategoryResponse>(CategoryErrors.NameAlreadyExists(newName));
            }

            // Invariant'lar Domain mutator'ında korunur (boş değil, azami uzunluk, UpdatedAt).
            category.Rename(newName);

            await categoryRepository.SaveChangesAsync(cancellationToken);

            // Blog sayısını yanıt için DB seviyesinde tekrar projekte et (Blogs koleksiyonu
            // tracked entity'de yüklü değildir). Kategori az önce güncellendiği için mevcuttur.
            CategoryWithBlogCount? projected =
                await categoryRepository.GetByIdWithBlogCountAsync(command.Id, cancellationToken);

            CategoryResponse response = projected is not null
                ? CategoryMapper.ToResponse(projected)
                : new CategoryResponse(
                    category.Id, category.CategoryName, 0, category.CreatedAt, category.UpdatedAt);

            return Result.Success(response);
        }
    }
}
