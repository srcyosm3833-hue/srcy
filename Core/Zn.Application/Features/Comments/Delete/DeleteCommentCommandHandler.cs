using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Comments.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Comments.Delete
{
    /// <summary>
    /// <see cref="DeleteCommentCommand"/>'ı işleyen Wolverine handler'ı. Yorum yoksa NotFound (404);
    /// istek sahibi yorumun sahibi değil ve Admin değilse Forbidden (403). Yetki geçilirse yorum
    /// silinir. Comment → SubComment ilişkisi DeleteBehavior.Cascade olduğundan alt yorumlar
    /// veritabanı tarafından otomatik silinir; handler'da ekstra silme işlemi gerekmez.
    /// </summary>
    public static class DeleteCommentCommandHandler
    {
        public static async Task<Result> Handle(
            DeleteCommentCommand command,
            ICommentRepository commentRepository,
            CancellationToken cancellationToken)
        {
            Comment? comment = await commentRepository.GetByIdAsync(command.Id, cancellationToken);
            if (comment is null)
            {
                return Result.Failure(CommentErrors.NotFound(command.Id));
            }

            // Yetki: yorumun sahibi veya Admin silebilir. NotFound önce, sonra yetki.
            bool isOwner = comment.UserId == command.RequestingUserId;
            if (!isOwner && !command.IsAdmin)
            {
                return Result.Failure(CommentErrors.ForbiddenDelete());
            }

            commentRepository.Remove(comment);
            await commentRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
