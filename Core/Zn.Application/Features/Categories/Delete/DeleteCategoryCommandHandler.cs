using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Categories.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Categories.Delete
{
    /// <summary>
    /// <see cref="DeleteCategoryCommand"/>'ı işleyen Wolverine handler'ı.
    /// Kategori yoksa NotFound (404). Blog → Category FK'sı Restrict olduğundan, bağlı blogu
    /// olan bir kategorinin silinmesi DB seviyesinde patlardı; bunun yerine handler önceden
    /// kontrol edip anlamlı bir Conflict (409) döndürür.
    /// </summary>
    public static class DeleteCategoryCommandHandler
    {
        public static async Task<Result> Handle(
            DeleteCategoryCommand command,
            ICategoryRepository categoryRepository,
            CancellationToken cancellationToken)
        {
            Category? category = await categoryRepository.GetByIdAsync(command.Id, cancellationToken);
            if (category is null)
            {
                return Result.Failure(CategoryErrors.NotFound(command.Id));
            }

            // Restrict FK: bağlı blog varsa DB ConstraintException atardı. Bunu kullanıcıya
            // ham 500 yerine anlamlı 409 olarak döndürmek için önceden kontrol ediyoruz.
            bool hasBlogs = await categoryRepository.HasBlogsAsync(command.Id, cancellationToken);
            if (hasBlogs)
            {
                return Result.Failure(CategoryErrors.HasBlogs(command.Id));
            }

            categoryRepository.Remove(category);
            await categoryRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
