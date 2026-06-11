using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.SubComments.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.SubComments.Delete
{
    /// <summary>
    /// <see cref="DeleteSubCommentCommand"/>'ı işleyen Wolverine handler'ı. Alt yorum yoksa
    /// NotFound (404); istek sahibi alt yorumun sahibi değil ve Admin değilse Forbidden (403).
    /// Yetki geçilirse alt yorum silinir.
    /// </summary>
    public static class DeleteSubCommentCommandHandler
    {
        public static async Task<Result> Handle(
            DeleteSubCommentCommand command,
            ISubCommentRepository subCommentRepository,
            CancellationToken cancellationToken)
        {
            SubComment? subComment = await subCommentRepository.GetByIdAsync(command.Id, cancellationToken);
            if (subComment is null)
            {
                return Result.Failure(SubCommentErrors.NotFound(command.Id));
            }

            // Yetki: alt yorumun sahibi veya Admin silebilir. NotFound önce, sonra yetki.
            bool isOwner = subComment.UserId == command.RequestingUserId;
            if (!isOwner && !command.IsAdmin)
            {
                return Result.Failure(SubCommentErrors.ForbiddenDelete());
            }

            subCommentRepository.Remove(subComment);
            await subCommentRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
