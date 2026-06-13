using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Comments.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Comments.Add
{
    /// <summary>
    /// <see cref="AddCommentCommand"/>'ı işleyen Wolverine handler'ı (plain metot konvansiyonu).
    /// Hedef blog yoksa NotFound (404); aksi halde <see cref="Comment.Create"/> factory'si ile
    /// invariant'lara uygun entity oluşturup kaydeder. Yazar token'dan gelen UserId'dir.
    /// Başarıda oluşturulan yorumun yanıtı döner (controller 201 olarak sunar).
    /// </summary>
    public static class AddCommentCommandHandler
    {
        public static async Task<Result<CommentResponse>> Handle(
            AddCommentCommand command,
            ICommentRepository commentRepository,
            CancellationToken cancellationToken)
        {
            // Var olmayan bloga yorum bağlanamaz. FK ihlaliyle ham 500 yerine anlamlı 404.
            bool blogExists = await commentRepository.BlogExistsAsync(command.BlogId, cancellationToken);
            if (!blogExists)
            {
                return Result.Failure<CommentResponse>(CommentErrors.BlogNotFound(command.BlogId));
            }

            // Invariant'lar (boş olmayan metin, azami uzunluk, geçerli blog/yazar) Domain factory'sinde korunur.
            Comment comment = Comment.Create(command.BlogId, command.UserId, command.CommentText);

            await commentRepository.AddAsync(comment, cancellationToken);
            await commentRepository.SaveChangesAsync(cancellationToken);

            // Yanıtı DB'den projeksiyonla döndür (yazar adı + alt yorum sayısı dahil).
            // Yorum az önce eklendiği için mevcuttur; null gelmesi beklenmez.
            // Yorumu yapanın kimliğini geçiyoruz; yeni yorumun hiç beğenisi olmadığı için
            // IsLikedByCurrentUser doğal olarak false döner.
            CommentListItem? created =
                await commentRepository.GetResponseByIdAsync(comment.Id, command.UserId, cancellationToken);

            CommentResponse response = created is not null
                ? CommentMapper.ToResponse(created)
                : new CommentResponse(
                    comment.Id,
                    comment.CommentText,
                    comment.UserId,
                    DisplayName: string.Empty,
                    comment.CreatedAt,
                    comment.UpdatedAt,
                    IsEdited: comment.UpdatedAt is not null,
                    SubCommentCount: 0,
                    LikeCount: 0,
                    IsLikedByCurrentUser: false);

            return Result.Success(response);
        }
    }
}
