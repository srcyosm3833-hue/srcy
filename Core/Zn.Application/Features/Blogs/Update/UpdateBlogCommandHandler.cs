using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Blogs.Common;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Blogs.Update
{
    /// <summary>
    /// <see cref="UpdateBlogCommand"/>'ı işleyen Wolverine handler'ı. Sırasıyla: blog var mı
    /// (404), istek sahibi yetkili mi — yazar veya Admin (403), yeni kategori var mı (400).
    /// Tüm kontroller geçilirse <see cref="Blog.Update"/> mutator'ı ile invariant korunarak
    /// güncellenir; UpdatedAt orada güncellenir.
    /// </summary>
    public static class UpdateBlogCommandHandler
    {
        public static async Task<Result<BlogDetailResponse>> Handle(
            UpdateBlogCommand command,
            IBlogRepository blogRepository,
            CancellationToken cancellationToken)
        {
            Blog? blog = await blogRepository.GetByIdAsync(command.Id, cancellationToken);
            if (blog is null)
            {
                return Result.Failure<BlogDetailResponse>(BlogErrors.NotFound(command.Id));
            }

            // Yetki: yalnızca yazar veya Admin güncelleyebilir. Var olmayan kaydı sızdırmamak
            // için NotFound kontrolü önce yapılır, ardından yetki.
            bool isAuthor = blog.UserId == command.RequestingUserId;
            if (!isAuthor && !command.IsAdmin)
            {
                return Result.Failure<BlogDetailResponse>(BlogErrors.Forbidden());
            }

            // Kategori değişmiş olabilir; hedef kategorinin varlığını doğrula.
            if (blog.CategoryId != command.CategoryId)
            {
                bool categoryExists =
                    await blogRepository.CategoryExistsAsync(command.CategoryId, cancellationToken);

                if (!categoryExists)
                {
                    return Result.Failure<BlogDetailResponse>(
                        BlogErrors.CategoryNotFound(command.CategoryId));
                }
            }

            // Invariant'lar Domain mutator'ında korunur (boş değil, azami uzunluk, UpdatedAt).
            blog.Update(
                command.Title,
                command.Description,
                command.CoverImage,
                command.BlogImage,
                command.CategoryId);

            await blogRepository.SaveChangesAsync(cancellationToken);

            // Güncel yanıtı tam projeksiyonla döndür (kategori adı + yazar adı dahil).
            // IsLikedByCurrentUser için isteği yapan kullanıcının kimliğini geçiyoruz.
            BlogDetail? detail =
                await blogRepository.GetDetailByIdAsync(blog.Id, command.RequestingUserId, cancellationToken);

            BlogDetailResponse response = detail is not null
                ? BlogMapper.ToDetailResponse(detail)
                : new BlogDetailResponse(
                    blog.Id,
                    blog.Title,
                    blog.CoverImage,
                    blog.BlogImage,
                    blog.Description,
                    blog.CategoryId,
                    CategoryName: string.Empty,
                    AuthorId: blog.UserId,
                    AuthorName: string.Empty,
                    blog.CreatedAt,
                    blog.UpdatedAt,
                    LikeCount: 0,
                    IsLikedByCurrentUser: false);

            return Result.Success(response);
        }
    }
}
