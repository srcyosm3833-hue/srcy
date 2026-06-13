using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Blogs.Common;
using Zn.Application.Interfaces.Persistence;

namespace Zn.Application.Features.Blogs.GetById
{
    /// <summary>
    /// <see cref="GetBlogAuditByIdQuery"/>'i işleyen Wolverine handler'ı. Blog audit detayı
    /// (CreatorIpHash dahil) DB'den projekte edilir; yoksa anlamlı 404 döner. Yetki (Admin/Manager)
    /// controller seviyesinde uygulanır. İş mantığı incedir.
    /// </summary>
    public static class GetBlogAuditByIdQueryHandler
    {
        public static async Task<Result<BlogAuditDetailResponse>> Handle(
            GetBlogAuditByIdQuery query,
            IBlogRepository blogRepository,
            CancellationToken cancellationToken)
        {
            BlogAuditDetail? blog =
                await blogRepository.GetAuditDetailByIdAsync(query.Id, cancellationToken);

            if (blog is null)
            {
                return Result.Failure<BlogAuditDetailResponse>(BlogErrors.NotFound(query.Id));
            }

            BlogAuditDetailResponse response = BlogMapper.ToAuditDetailResponse(blog);

            return Result.Success(response);
        }
    }
}
