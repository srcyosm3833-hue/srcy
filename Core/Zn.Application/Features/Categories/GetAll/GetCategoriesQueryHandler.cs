using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Categories.Common;
using Zn.Application.Interfaces.Persistence;

namespace Zn.Application.Features.Categories.GetAll
{
    /// <summary>
    /// <see cref="GetCategoriesQuery"/>'i işleyen Wolverine handler'ı.
    /// Repository, blog sayısını veritabanı seviyesinde projekte eder; handler yalnızca
    /// Mapperly ile DTO'ya çevirip döner. Liste boş olabilir (her zaman Success).
    /// </summary>
    public static class GetCategoriesQueryHandler
    {
        public static async Task<Result<IReadOnlyList<CategoryResponse>>> Handle(
            GetCategoriesQuery query,
            ICategoryRepository categoryRepository,
            CancellationToken cancellationToken)
        {
            // Public liste: silinmiş kategoriler gösterilmez (includeDeleted=false). Admin'in
            // silinmişleri görmesi için includeDeleted'i query'ye taşıyan dilim Özellik 5'te eklenir.
            IReadOnlyList<CategoryWithBlogCount> categories =
                await categoryRepository.GetAllWithBlogCountAsync(false, cancellationToken);

            IReadOnlyList<CategoryResponse> response = CategoryMapper.ToResponseList(categories);

            return Result.Success(response);
        }
    }
}
