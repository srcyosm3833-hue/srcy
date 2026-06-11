using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Comments.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Comments.Update
{
    /// <summary>
    /// <see cref="UpdateCommentCommand"/>'ı işleyen Wolverine handler'ı. Sırasıyla: yorum var mı
    /// (404), istek sahibi yorumun sahibi mi (değilse 403 — Admin bile başkasının yorumunu
    /// düzenleyemez). Kontroller geçilirse <see cref="Comment.Update"/> mutator'ı ile invariant
    /// korunarak güncellenir; UpdatedAt orada set edilir ve dönen yanıtta isEdited true olur.
    /// </summary>
    public static class UpdateCommentCommandHandler
    {
        public static async Task<Result<CommentResponse>> Handle(
            UpdateCommentCommand command,
            ICommentRepository commentRepository,
            CancellationToken cancellationToken)
        {
            Comment? comment = await commentRepository.GetByIdAsync(command.Id, cancellationToken);
            if (comment is null)
            {
                return Result.Failure<CommentResponse>(CommentErrors.NotFound(command.Id));
            }

            // Yetki: yalnızca sahibi düzenleyebilir (Admin override YOK). Var olmayan kaydı
            // sızdırmamak için NotFound kontrolü önce yapılır, ardından yetki.
            bool isOwner = comment.UserId == command.RequestingUserId;
            if (!isOwner)
            {
                return Result.Failure<CommentResponse>(CommentErrors.ForbiddenEdit());
            }

            // Invariant'lar Domain mutator'ında korunur (boş değil, azami uzunluk, UpdatedAt).
            comment.Update(command.CommentText);

            await commentRepository.SaveChangesAsync(cancellationToken);

            // Güncel yanıtı projeksiyonla döndür (yazar adı + alt yorum sayısı dahil).
            CommentListItem? updated = await commentRepository.GetResponseByIdAsync(comment.Id, cancellationToken);

            CommentResponse response = updated is not null
                ? CommentMapper.ToResponse(updated)
                : new CommentResponse(
                    comment.Id,
                    comment.CommentText,
                    comment.UserId,
                    DisplayName: string.Empty,
                    comment.CreatedAt,
                    comment.UpdatedAt,
                    IsEdited: comment.UpdatedAt is not null,
                    SubCommentCount: 0);

            return Result.Success(response);
        }
    }
}
