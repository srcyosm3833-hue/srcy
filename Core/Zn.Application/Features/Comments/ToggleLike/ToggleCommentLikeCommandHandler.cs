using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Comments.Common;
using Zn.Application.Interfaces.Persistence;

namespace Zn.Application.Features.Comments.ToggleLike
{
    /// <summary>
    /// <see cref="ToggleCommentLikeCommand"/>'ı işleyen Wolverine handler'ı. Önce yorum varlığını
    /// doğrular (yoksa 404); ardından beğeniyi açıp kapatır. Toggle'ın atomik/idempotent davranışı
    /// (eş zamanlı çift istekte duplicate üretmeme) repository'de garanti edilir — composite PK
    /// (CommentId, UserId) ve eş zamanlılık hatası ele alma orada yapılır. Handler ince tutulur.
    /// </summary>
    public static class ToggleCommentLikeCommandHandler
    {
        public static async Task<Result<CommentLikeToggleResponse>> Handle(
            ToggleCommentLikeCommand command,
            ICommentLikeRepository commentLikeRepository,
            CancellationToken cancellationToken)
        {
            // Önce 404: var olmayan yoruma beğeni atılamaz.
            bool commentExists =
                await commentLikeRepository.CommentExistsAsync(command.CommentId, cancellationToken);
            if (!commentExists)
            {
                return Result.Failure<CommentLikeToggleResponse>(CommentErrors.NotFound(command.CommentId));
            }

            (bool liked, int likeCount) =
                await commentLikeRepository.ToggleAsync(command.CommentId, command.UserId, cancellationToken);

            return Result.Success(new CommentLikeToggleResponse(liked, likeCount));
        }
    }
}
