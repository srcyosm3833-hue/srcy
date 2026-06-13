using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Zn.Application.Common.Results;
using Zn.Application.Features.Blogs.Common;
using Zn.Application.Features.Blogs.GetById;
using Zn.Domain.Authorization;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// Blog yönetimi (Admin/Manager) audit uç noktaları (<c>api/admin/blogs</c>). Public
    /// <see cref="BlogsController"/>'dan farkı: audit alanı <c>creatorIpHash</c>'i içeren detay döner.
    /// Tüm action'lar <c>[Authorize(Roles = "Admin,Manager")]</c> ile korunur — token yoksa 401, rol
    /// yetersizse 403 (A6: blog yönetimi Admin + Manager). Audit alanı yalnızca bu controller'dan döner;
    /// public uçlarda (<c>GET /api/blogs</c>, <c>GET /api/blogs/{id}</c>) ASLA yer almaz.
    /// </summary>
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Manager}")]
    [Route("api/admin/blogs")]
    public sealed class AdminBlogsController : ApiControllerBase
    {
        private readonly IMessageBus _messageBus;

        public AdminBlogsController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        /// <summary>
        /// Tek bir blogu admin audit detayıyla (creatorIpHash dahil) döner. Soft delete edilmiş bloglar
        /// da denetlenebilir (global query filter bypass). Bulunamazsa 404. Başarıda 200.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BlogAuditDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAuditById(Guid id, CancellationToken cancellationToken)
        {
            Result<BlogAuditDetailResponse> result =
                await _messageBus.InvokeAsync<Result<BlogAuditDetailResponse>>(
                    new GetBlogAuditByIdQuery(id), cancellationToken);

            return HandleResult(result);
        }
    }
}
