using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.SubComments.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.SubComments.Add
{
    /// <summary>
    /// <see cref="AddSubCommentCommand"/>'ı işleyen Wolverine handler'ı (plain metot konvansiyonu).
    /// Hedef ana yorum yoksa NotFound (404); aksi halde <see cref="SubComment.Create"/> factory'si
    /// ile invariant'lara uygun entity oluşturup kaydeder. Yazar token'dan gelen UserId'dir.
    /// Başarıda oluşturulan alt yorumun yanıtı döner (controller 201 olarak sunar).
    /// </summary>
    public static class AddSubCommentCommandHandler
    {
        public static async Task<Result<SubCommentResponse>> Handle(
            AddSubCommentCommand command,
            ISubCommentRepository subCommentRepository,
            CancellationToken cancellationToken)
        {
            // Var olmayan yoruma yanıt bağlanamaz. FK ihlaliyle ham 500 yerine anlamlı 404.
            bool commentExists =
                await subCommentRepository.CommentExistsAsync(command.CommentId, cancellationToken);
            if (!commentExists)
            {
                return Result.Failure<SubCommentResponse>(
                    SubCommentErrors.CommentNotFound(command.CommentId));
            }

            // Invariant'lar (boş olmayan metin, azami uzunluk, geçerli yorum/yazar) Domain factory'sinde korunur.
            SubComment subComment =
                SubComment.Create(command.CommentId, command.UserId, command.SubCommentText);

            await subCommentRepository.AddAsync(subComment, cancellationToken);
            await subCommentRepository.SaveChangesAsync(cancellationToken);

            // Yanıtı DB'den projeksiyonla döndür (yazar adı dahil).
            SubCommentListItem? created =
                await subCommentRepository.GetResponseByIdAsync(subComment.Id, cancellationToken);

            SubCommentResponse response = created is not null
                ? SubCommentMapper.ToResponse(created)
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
