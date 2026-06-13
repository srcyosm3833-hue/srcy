using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Blogs.Common;
using Zn.Application.Interfaces.Persistence;

namespace Zn.Application.Features.Blogs.ToggleLike
{
    /// <summary>
    /// <see cref="ToggleBlogLikeCommand"/>'ı işleyen Wolverine handler'ı. Önce blog varlığını
    /// doğrular (yoksa 404); ardından beğeniyi açıp kapatır. Toggle'ın atomik/idempotent davranışı
    /// (eş zamanlı çift istekte duplicate üretmeme) repository'de garanti edilir — composite PK
    /// (BlogId, UserId) ve eş zamanlılık hatası ele alma orada yapılır. Handler ince tutulur.
    /// </summary>
    public static class ToggleBlogLikeCommandHandler
    {
        public static async Task<Result<BlogLikeToggleResponse>> Handle(
            ToggleBlogLikeCommand command,
            IBlogLikeRepository blogLikeRepository,
            CancellationToken cancellationToken)
        {
            // Önce 404: var olmayan (veya soft-delete edilmiş) bloga beğeni atılamaz.
            bool blogExists = await blogLikeRepository.BlogExistsAsync(command.BlogId, cancellationToken);
            if (!blogExists)
            {
                return Result.Failure<BlogLikeToggleResponse>(BlogErrors.NotFound(command.BlogId));
            }

            (bool liked, int likeCount) =
                await blogLikeRepository.ToggleAsync(command.BlogId, command.UserId, cancellationToken);

            return Result.Success(new BlogLikeToggleResponse(liked, likeCount));
        }
    }
}
