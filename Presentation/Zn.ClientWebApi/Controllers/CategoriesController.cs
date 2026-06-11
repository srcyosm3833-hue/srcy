using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Zn.Application.Common.Results;
using Zn.Application.Features.Categories.Common;
using Zn.Application.Features.Categories.GetAll;
using Zn.Application.Features.Categories.GetById;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// Kategorilerin herkese açık okuma uç noktaları (kimlik doğrulama gerekmez).
    /// Sorgular Wolverine üzerinden handler'lara gönderilir; dönen <see cref="Result"/>'lar
    /// <see cref="ApiControllerBase"/> ile HTTP'ye eşlenir. Yazma işlemleri için
    /// <see cref="AdminCategoriesController"/>'a bakınız.
    /// </summary>
    [Route("api/categories")]
    public sealed class CategoriesController : ApiControllerBase
    {
        private readonly IMessageBus _messageBus;

        public CategoriesController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        /// <summary>Tüm kategorileri (her birinde blog sayısıyla) döner. Başarıda 200.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<CategoryResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            Result<IReadOnlyList<CategoryResponse>> result =
                await _messageBus.InvokeAsync<Result<IReadOnlyList<CategoryResponse>>>(
                    new GetCategoriesQuery(), cancellationToken);

            return HandleResult(result);
        }

        /// <summary>Tek bir kategoriyi Id ile döner. Bulunamazsa 404.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            Result<CategoryResponse> result =
                await _messageBus.InvokeAsync<Result<CategoryResponse>>(
                    new GetCategoryByIdQuery(id), cancellationToken);

            return HandleResult(result);
        }
    }
}
