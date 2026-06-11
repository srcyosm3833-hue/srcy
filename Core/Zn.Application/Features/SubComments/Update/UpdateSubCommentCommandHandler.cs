using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.SubComments.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.SubComments.Update
{
    /// <summary>
    /// <see cref="UpdateSubCommentCommand"/>'ı işleyen Wolverine handler'ı. Sırasıyla: alt yorum
    /// var mı (404), istek sahibi alt yorumun sahibi mi (değilse 403 — Admin bile düzenleyemez).
    /// Kontroller geçilirse <see cref="SubComment.Update"/> mutator'ı ile invariant korunarak
    /// güncellenir; UpdatedAt orada set edilir ve dönen yanıtta isEdited true olur.
    /// </summary>
    public static class UpdateSubCommentCommandHandler
    {
        public static async Task<Result<SubCommentResponse>> Handle(
            UpdateSubCommentCommand command,
            ISubCommentRepository subCommentRepository,
            CancellationToken cancellationToken)
        {
            SubComment? subComment = await subCommentRepository.GetByIdAsync(command.Id, cancellationToken);
            if (subComment is null)
            {
                return Result.Failure<SubCommentResponse>(SubCommentErrors.NotFound(command.Id));
            }

            // Yetki: yalnızca sahibi düzenleyebilir (Admin override YOK). NotFound önce, sonra yetki.
            bool isOwner = subComment.UserId == command.RequestingUserId;
            if (!isOwner)
            {
                return Result.Failure<SubCommentResponse>(SubCommentErrors.ForbiddenEdit());
            }

            // Invariant'lar Domain mutator'ında korunur (boş değil, azami uzunluk, UpdatedAt).
            subComment.Update(command.SubCommentText);

            await subCommentRepository.SaveChangesAsync(cancellationToken);

            // Güncel yanıtı projeksiyonla döndür (yazar adı dahil).
            SubCommentListItem? updated =
                await subCommentRepository.GetResponseByIdAsync(subComment.Id, cancellationToken);

            SubCommentResponse response = updated is not null
                ? SubCommentMapper.ToResponse(updated)
                : new SubCommentResponse(
                    subComment.Id,
                    subComment.SubCommentText,
                    subComment.CommentId,
                    subComment.UserId,
                    DisplayName: string.Empty,
                    subComment.CreatedAt,
                    subComment.UpdatedAt,
                    IsEdited: subComment.UpdatedAt is not null);

            return Result.Success(response);
        }
    }
}
