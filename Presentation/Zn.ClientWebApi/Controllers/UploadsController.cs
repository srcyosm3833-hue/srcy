using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Zn.Application.Common.Results;
using Zn.Application.Features.Uploads;
using Zn.Application.Interfaces.Storage;

namespace Zn.ClientWebApi.Controllers
{
    /// <summary>
    /// Dosya (görsel) yükleme uç noktası. Giriş yapmış kullanıcılar bir görsel yükleyip
    /// erişilebilir bir URL alır; bu URL daha sonra blog create/update'te CoverImage/BlogImage
    /// olarak kullanılır (blog alanları URL string kabul eder — KARAR A1).
    /// <para>
    /// Controller, ASP.NET'in <c>IFormFile</c>'ını framework'ten bağımsız
    /// <see cref="FileUploadRequest"/>'e çevirir; Application/Infrastructure IFormFile bilmez.
    /// </para>
    /// </summary>
    [Authorize]
    [Route("api/uploads")]
    public sealed class UploadsController : ApiControllerBase
    {
        private readonly IMessageBus _messageBus;

        public UploadsController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        /// <summary>
        /// Bir görseli yükler (multipart/form-data, "file" alanı). Başarıda 201 + erişilebilir URL.
        /// Boş/çok büyük/desteklenmeyen tür → 400; token yoksa 401.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(UploadImageResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Upload(IFormFile? file, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "No file was provided."
                });
            }

            // IFormFile → framework'ten bağımsız istek nesnesi (akış handler boyunca taşınır).
            var uploadRequest = new FileUploadRequest(
                file.OpenReadStream(),
                file.FileName,
                file.ContentType,
                file.Length);

            Result<UploadImageResponse> result =
                await _messageBus.InvokeAsync<Result<UploadImageResponse>>(
                    new UploadImageCommand(uploadRequest), cancellationToken);

            return HandleResult(result, value => StatusCode(StatusCodes.Status201Created, value));
        }
    }
}
