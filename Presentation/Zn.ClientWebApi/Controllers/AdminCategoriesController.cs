using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Zn.Application.Common.Results;
using Zn.Application.Features.Categories.Common;
using Zn.Application.Features.Categories.Create;
using Zn.Application.Features.Categories.Delete;
using Zn.Application.Features.Categories.Update;
using Zn.Domain.Authorization;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// Kategorilerin yönetici (Admin) yazma uç noktaları: oluştur, güncelle, sil.
    /// Tüm action'lar <c>[Authorize(Roles = "Admin")]</c> ile korunur — token yoksa 401,
    /// rol yetersizse 403. Komutlar Wolverine üzerinden handler'lara gönderilir;
    /// dönen <see cref="Result"/>'lar <see cref="ApiControllerBase"/> ile HTTP'ye eşlenir.
    /// </summary>
    [Authorize(Roles = RoleNames.Admin)]
    [Route("api/admin/categories")]
    public sealed class AdminCategoriesController : ApiControllerBase
    {
        private readonly IMessageBus _messageBus;

        public AdminCategoriesController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        /// <summary>Yeni kategori oluşturur. Başarıda 201; aynı isim varsa 409.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create(
            [FromBody] CreateCategoryCommand command,
            CancellationToken cancellationToken)
        {
            Result<CategoryResponse> result =
                await _messageBus.InvokeAsync<Result<CategoryResponse>>(command, cancellationToken);

            return HandleResult(result, value =>
                CreatedAtAction(
                    actionName: nameof(CategoriesController.GetById),
                    controllerName: "Categories",
                    routeValues: new { id = value.Id },
                    value: value));
        }

        /// <summary>Var olan kategoriyi günceller. Bulunamazsa 404; isim çakışırsa 409.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateCategoryRequest request,
            CancellationToken cancellationToken)
        {
            // Route'taki id otoritatiftir; gövdedeki ad ile birleştirilip komut oluşturulur.
            var command = new UpdateCategoryCommand(id, request.CategoryName);

            Result<CategoryResponse> result =
                await _messageBus.InvokeAsync<Result<CategoryResponse>>(command, cancellationToken);

            return HandleResult(result);
        }

        /// <summary>Kategoriyi siler. Bulunamazsa 404; bağlı blog varsa 409. Başarıda 204.</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            Result result =
                await _messageBus.InvokeAsync<Result>(new DeleteCategoryCommand(id), cancellationToken);

            return HandleResult(result);
        }

        /// <summary>
        /// PUT gövdesi: yalnızca yeni adı taşır. Id route'tan geldiği için gövdede
        /// tekrar edilmez (çelişki/güvenlik riskini önler).
        /// </summary>
        /// <param name="CategoryName">Kategorinin yeni adı.</param>
        public sealed record UpdateCategoryRequest(string CategoryName);
    }
}
