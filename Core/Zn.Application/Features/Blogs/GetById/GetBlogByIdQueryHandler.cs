using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Blogs.Common;
using Zn.Application.Interfaces.Persistence;

namespace Zn.Application.Features.Blogs.GetById
{
    /// <summary>
    /// <see cref="GetBlogByIdQuery"/>'i işleyen Wolverine handler'ı. Blog detayı DB'den
    /// projekte edilir; yoksa anlamlı 404 döner. İş mantığı incedir.
    /// </summary>
    public static class GetBlogByIdQueryHandler
    {
        public static async Task<Result<BlogDetailResponse>> Handle(
            GetBlogByIdQuery query,
            IBlogRepository blogRepository,
            CancellationToken cancellationToken)
        {
            BlogDetail? blog =
                await blogRepository.GetDetailByIdAsync(query.Id, query.CurrentUserId, cancellationToken);

            if (blog is null)
            {
                return Result.Failure<BlogDetailResponse>(BlogErrors.NotFound(query.Id));
            }

            BlogDetailResponse response = BlogMapper.ToDetailResponse(blog);

            return Result.Success(response);
        }
    }
}
