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
using Zn.Application.Features.Blogs.Update;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// Blog uç noktaları. Okuma (GET) işlemleri herkese açıktır; yazma (POST/PUT/DELETE)
    /// işlemleri kimlik doğrulama gerektirir ve "yazar veya Admin" yetki modeline tabidir.
    /// <para>
    /// Yazma işlemleri salt Admin'e değil, giriş yapmış her kullanıcıya (kendi blogları için)
    /// açık olduğundan ayrı bir Admin controller'ı yerine bu controller'da metot seviyesinde
    /// <c>[Authorize]</c> kullanılır. Yazar kimliği ve Admin rolü token'dan okunur — istek
    /// gövdesinden ASLA alınmaz.
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
                    new GetBlogsQuery(page, pageSize, categoryId), cancellationToken);

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
                    new GetBlogByIdQuery(id), cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// Yeni blog oluşturur. Giriş yapmış her kullanıcı yazar olabilir; yazar kimliği
        /// token'dan alınır. Başarıda 201; kategori yoksa 400; token yoksa 401.
        /// </summary>
        [HttpPost]
        [Authorize]
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
        /// Var olan blogu günceller. Yalnızca yazarı veya Admin yapabilir (aksi halde 403).
        /// Bulunamazsa 404; kategori yoksa 400.
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize]
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
        /// Blogu siler. Yalnızca yazarı veya Admin yapabilir (aksi halde 403). Bulunamazsa 404.
        /// Başarıda 204.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize]
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
