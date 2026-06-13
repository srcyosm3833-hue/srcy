using System.Threading;
using System.Threading.Tasks;
using Zn.Application.Common.Results;
using Zn.Application.Features.Blogs.Common;
using Zn.Application.Interfaces.Audit;
using Zn.Application.Interfaces.Authentication;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Blogs.Create
{
    /// <summary>
    /// <see cref="CreateBlogCommand"/>'ı işleyen Wolverine handler'ı (plain metot konvansiyonu).
    /// Seçilen kategori yoksa Validation (400) döner; aksi halde <see cref="Blog.Create"/>
    /// factory'si ile invariant'lara uygun entity oluşturup kaydeder. Yazar token'dan gelen
    /// UserId'dir. Başarıda oluşturulan blogun detay yanıtı döner (controller 201 olarak sunar).
    /// <para>
    /// Audit (anonim): Oluşturan istemcinin IP adresi <see cref="IClientIpResolver"/> ile çözülür,
    /// <see cref="IIpHasher"/> ile tuzlu hash'lenir (ham IP saklanmaz) ve <see cref="Blog.Create"/>
    /// factory'sine geçirilir. IP çözülemezse hash null olur ve blog yine de oluşturulur (hata yok).
    /// Bu alan yalnızca admin audit görünümünde döner; public yanıtlarda yer almaz.
    /// </para>
    /// </summary>
    public static class CreateBlogCommandHandler
    {
        public static async Task<Result<BlogDetailResponse>> Handle(
            CreateBlogCommand command,
            IBlogRepository blogRepository,
            IClientIpResolver clientIpResolver,
            IIpHasher ipHasher,
            CancellationToken cancellationToken)
        {
            // Kategori varlık kontrolü: var olmayan kategoriye blog bağlanamaz.
            // FK ihlaliyle ham 500 yerine anlamlı 400 döndürmek için önceden kontrol ederiz.
            bool categoryExists =
                await blogRepository.CategoryExistsAsync(command.CategoryId, cancellationToken);

            if (!categoryExists)
            {
                return Result.Failure<BlogDetailResponse>(
                    BlogErrors.CategoryNotFound(command.CategoryId));
            }

            // İstemci IP'sini çöz ve saklamadan ÖNCE hash'le; çözülemezse null (audit opsiyonel,
            // blog yine de oluşur). Ham IP hiçbir zaman entity'ye/DB'ye geçmez.
            string? ipHash = ipHasher.Hash(clientIpResolver.ResolveIpAddress());

            // Invariant'lar (boş olmayan alanlar, azami uzunluk, geçerli yazar/kategori) Domain factory'sinde korunur.
            Blog blog = Blog.Create(
                command.Title,
                command.Description,
                command.CoverImage,
                command.BlogImage,
                command.CategoryId,
                command.UserId,
                ipHash);

            await blogRepository.AddAsync(blog, cancellationToken);
            await blogRepository.SaveChangesAsync(cancellationToken);

            // Yanıtı DB'den tam projeksiyonla döndür (kategori adı + yazar adı dahil).
            // Blog az önce eklendiği için mevcuttur; null gelmesi beklenmez.
            // Yazarın kendi kimliğini geçiyoruz; yeni blogun hiç beğenisi olmadığı için
            // IsLikedByCurrentUser doğal olarak false döner.
            BlogDetail? detail =
                await blogRepository.GetDetailByIdAsync(blog.Id, command.UserId, cancellationToken);

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
