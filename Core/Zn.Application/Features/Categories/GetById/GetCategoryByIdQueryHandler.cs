using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Categories.Common;
using Zn.Application.Interfaces.Persistence;

namespace Zn.Application.Features.Categories.GetById
{
    /// <summary>
    /// <see cref="GetCategoryByIdQuery"/>'i işleyen Wolverine handler'ı.
    /// Kategori (blog sayısıyla) DB'den projekte edilir; yoksa anlamlı 404 döner.
    /// </summary>
    public static class GetCategoryByIdQueryHandler
    {
        public static async Task<Result<CategoryResponse>> Handle(
            GetCategoryByIdQuery query,
            ICategoryRepository categoryRepository,
            CancellationToken cancellationToken)
        {
            CategoryWithBlogCount? category =
                await categoryRepository.GetByIdWithBlogCountAsync(query.Id, cancellationToken);

            if (category is null)
            {
                return Result.Failure<CategoryResponse>(CategoryErrors.NotFound(query.Id));
            }

            CategoryResponse response = CategoryMapper.ToResponse(category);

            return Result.Success(response);
        }
    }
}
