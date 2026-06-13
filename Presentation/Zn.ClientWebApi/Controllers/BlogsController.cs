using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Zn.Application.Common.Pagination;
using Zn.Application.Common.Results;
using Zn.Application.Features.Blogs.Common;
using Zn.Application.Features.Blogs.Create;
using Zn.Application.Features.Blogs.Delete;
using Zn.Application.Features.Blogs.GetAll;
using Zn.Application.Features.Blogs.GetById;
using Zn.Application.Features.Blogs.Search;
using Zn.Application.Features.Blogs.ToggleLike;
using Zn.Application.Features.Blogs.Update;
using Zn.Domain.Authorization;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// Blog uç noktaları. Okuma (GET) işlemleri herkese açıktır; yazma (POST/PUT/DELETE)
    /// işlemleri yalnızca Admin veya Manager rolündeki kullanıcılara açıktır (A6 yetki matrisi).
    /// <para>
    /// Yazma uçları metot seviyesinde <c>[Authorize(Roles = "Admin,Manager")]</c> ile korunur:
    /// token yoksa 401, rol yetersizse 403. Yetki ayrımı handler'da incelenir — Admin herhangi
    /// bir blogu, Manager (ve yazar) yalnızca kendi blogunu güncelleyebilir/silebilir. Yazar
    /// kimliği ve Admin rolü token'dan okunur — istek gövdesinden ASLA alınmaz.
    /// </para>
    /// </summary>
    [Route("api/blogs")]
    public sealed class BlogsController : ApiControllerBase
    {
        private readonly IMessageBus _messageBus;

        public BlogsController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        /// <summary>
        /// Blogları sayfalı (createdAt azalan) ve opsiyonel kategori filtresiyle döner. Başarıda 200.
        /// pageSize üst sınırı handler'da uygulanır.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<BlogListItemResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? categoryId = null,
            CancellationToken cancellationToken = default)
        {
            Result<PagedResult<BlogListItemResponse>> result =
                await _messageBus.InvokeAsync<Result<PagedResult<BlogListItemResponse>>>(
                    new GetBlogsQuery(page, pageSize, categoryId, GetUserId()), cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// Blogları serbest metin (<paramref name="q"/>) ile başlık/açıklamada arar; sayfalı
        /// ve opsiyonel kategori filtreli döner. Herkese açıktır. Başarıda 200; arama terimi
        /// boş/whitespace ise veya 200 karakteri aşarsa 400. Silinmiş bloglar sonuçta yer almaz.
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<BlogListItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Search(
            [FromQuery] string q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? categoryId = null,
            CancellationToken cancellationToken = default)
        {
            Result<PagedResult<BlogListItemResponse>> result =
                await _messageBus.InvokeAsync<Result<PagedResult<BlogListItemResponse>>>(
                    new SearchBlogsQuery(q, page, pageSize, categoryId, GetUserId()), cancellationToken);

            return HandleResult(result);
        }

        /// <summary>Tek bir blogu Id ile tam detayıyla döner. Bulunamazsa 404.</summary>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BlogDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            Result<BlogDetailResponse> result =
                await _messageBus.InvokeAsync<Result<BlogDetailResponse>>(
                    new GetBlogByIdQuery(id, GetUserId()), cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// Yeni blog oluşturur. Yalnızca Admin veya Manager yazar olabilir; yazar kimliği
        /// token'dan alınır. Başarıda 201; kategori yoksa 400; token yoksa 401; rol yetersizse 403.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Manager}")]
        [ProducesResponseType(typeof(BlogDetailResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create(
            [FromBody] CreateBlogRequest request,
            CancellationToken cancellationToken)
        {
            // Yazar token'dan; gövdedeki hiçbir UserId alanı kabul edilmez.
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var command = new CreateBlogCommand(
                request.Title,
                request.Description,
                request.CoverImage,
                request.BlogImage,
                request.CategoryId,
                userId);

            Result<BlogDetailResponse> result =
                await _messageBus.InvokeAsync<Result<BlogDetailResponse>>(command, cancellationToken);

            return HandleResult(result, value =>
                CreatedAtAction(nameof(GetById), new { id = value.Id }, value));
        }

        /// <summary>
        /// Var olan blogu günceller. Admin/Manager erişebilir; ayrıca handler'da yazar/Admin ayrımı
        /// uygulanır: Admin tüm blogları, Manager (yazar olarak) yalnızca kendi blogunu güncelleyebilir
        /// (aksi halde 403). Bulunamazsa 404; kategori yoksa 400.
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Manager}")]
        [ProducesResponseType(typeof(BlogDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateBlogRequest request,
            CancellationToken cancellationToken)
        {
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var command = new UpdateBlogCommand(
                id,
                request.Title,
                request.Description,
                request.CoverImage,
                request.BlogImage,
                request.CategoryId,
                userId,
                IsAdmin());

            Result<BlogDetailResponse> result =
                await _messageBus.InvokeAsync<Result<BlogDetailResponse>>(command, cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// Blogu (soft) siler. Admin/Manager erişebilir; ayrıca handler'da yazar/Admin ayrımı
        /// uygulanır: Admin tüm blogları, Manager (yazar olarak) yalnızca kendi blogunu silebilir
        /// (aksi halde 403). Bulunamazsa 404. Başarıda 204.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Manager}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            Result result = await _messageBus.InvokeAsync<Result>(
                new DeleteBlogCommand(id, userId, IsAdmin()), cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// Blogun beğenisini açıp kapatır (toggle). Giriş yapmış her kullanıcı kullanabilir; beğeniyi
        /// yapan kullanıcı token'dan alınır (gövdeden ASLA). Mevcut beğeni varsa kaldırılır, yoksa
        /// eklenir; işlem idempotenttir. Başarıda 200 + { liked, likeCount }; blog yoksa 404; token
        /// yoksa 401.
        /// </summary>
        [HttpPost("{id:guid}/like")]
        [Authorize]
        [ProducesResponseType(typeof(BlogLikeToggleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleLike(Guid id, CancellationToken cancellationToken)
        {
            // Beğeniyi yapan token'dan; gövdedeki hiçbir UserId alanı kabul edilmez.
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            Result<BlogLikeToggleResponse> result =
                await _messageBus.InvokeAsync<Result<BlogLikeToggleResponse>>(
                    new ToggleBlogLikeCommand(id, userId), cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// POST/PUT gövdesi: blogun içerik alanları. UserId bilinçli olarak yoktur — yazar
        /// token'dan alınır, gövdeden alınmaz.
        /// </summary>
        public sealed record CreateBlogRequest(
            string Title,
            string Description,
            string CoverImage,
            string BlogImage,
            Guid CategoryId);

        /// <summary>PUT gövdesi: güncellenecek içerik alanları (Id route'tan gelir).</summary>
        public sealed record UpdateBlogRequest(
            string Title,
            string Description,
            string CoverImage,
            string BlogImage,
            Guid CategoryId);
    }
}
