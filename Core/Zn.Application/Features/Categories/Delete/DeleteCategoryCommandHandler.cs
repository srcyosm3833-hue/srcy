using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Categories.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Categories.Delete
{
    /// <summary>
    /// <see cref="DeleteCategoryCommand"/>'ı işleyen Wolverine handler'ı. Kategori yoksa NotFound (404).
    /// <para>
    /// Silme artık <b>soft delete</b>'tir: kayıt kalıcı silinmez, <see cref="Category.SoftDelete"/>
    /// ile IsDeleted=true / DeletedAt set edilir. Bu nedenle Blog → Category Restrict FK kısıtı
    /// tetiklenmez — bağlı blogu olan kategori de sorunsuz soft delete edilebilir (US-12). Eski
    /// "bağlı blog varsa 409" ön kontrolü kaldırıldı.
    /// </para>
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

            category.SoftDelete();
            await categoryRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
