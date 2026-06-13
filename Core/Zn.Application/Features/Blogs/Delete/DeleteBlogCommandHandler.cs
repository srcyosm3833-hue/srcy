using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Blogs.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Blogs.Delete
{
    /// <summary>
    /// <see cref="DeleteBlogCommand"/>'ı işleyen Wolverine handler'ı. Blog yoksa NotFound (404);
    /// istek sahibi yazar değil ve Admin değilse Forbidden (403). Yetki geçilirse blog
    /// <b>soft delete</b> edilir: kayıt kalıcı silinmez, <see cref="Blog.SoftDelete"/> ile
    /// IsDeleted=true / DeletedAt set edilir ve global query filter sayesinde sonraki sorgularda
    /// görünmez.
    /// </summary>
    public static class DeleteBlogCommandHandler
    {
        public static async Task<Result> Handle(
            DeleteBlogCommand command,
            IBlogRepository blogRepository,
            CancellationToken cancellationToken)
        {
            Blog? blog = await blogRepository.GetByIdAsync(command.Id, cancellationToken);
            if (blog is null)
            {
                return Result.Failure(BlogErrors.NotFound(command.Id));
            }

            // Yetki: yalnızca yazar veya Admin silebilir.
            bool isAuthor = blog.UserId == command.RequestingUserId;
            if (!isAuthor && !command.IsAdmin)
            {
                return Result.Failure(BlogErrors.Forbidden());
            }

            blog.SoftDelete();
            await blogRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
