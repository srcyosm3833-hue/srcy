using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Categories.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Categories.Create
{
    /// <summary>
    /// <see cref="CreateCategoryCommand"/>'ı işleyen Wolverine handler'ı (plain metot konvansiyonu).
    /// Aynı isimde kategori varsa Conflict (409) döner; yoksa <see cref="Category.Create"/>
    /// factory'si ile invariant'lara uygun entity oluşturup kaydeder. İş mantığı incedir.
    /// </summary>
    public static class CreateCategoryCommandHandler
    {
        public static async Task<Result<CategoryResponse>> Handle(
            CreateCategoryCommand command,
            ICategoryRepository categoryRepository,
            CancellationToken cancellationToken)
        {
            string categoryName = command.CategoryName.Trim();

            // Uygulama seviyesinde erken duplicate kontrolü: DB unique index'i son savunma
            // hattıdır, ama anlamlı 409 döndürmek için önce burada kontrol ederiz.
            bool exists = await categoryRepository.ExistsByNameAsync(
                categoryName, excludeId: null, cancellationToken);

            if (exists)
            {
                return Result.Failure<CategoryResponse>(
                    CategoryErrors.NameAlreadyExists(categoryName));
            }

            // Invariant'lar (boş değil, azami uzunluk) Domain factory'sinde korunur.
            Category category = Category.Create(categoryName);

            await categoryRepository.AddAsync(category, cancellationToken);
            await categoryRepository.SaveChangesAsync(cancellationToken);

            var response = new CategoryResponse(
                category.Id,
                category.CategoryName,
                BlogCount: 0,
                category.CreatedAt,
                category.UpdatedAt);

            return Result.Success(response);
        }
    }
}
