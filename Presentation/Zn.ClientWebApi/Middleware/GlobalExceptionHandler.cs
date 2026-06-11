using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Zn.ClientWebApi.Middleware
{
    /// <summary>
    /// .NET 8+ IExceptionHandler idiomuyla yazılmış global hata yakalayıcı.
    /// İşlenmemiş exception'ları RFC 7807 ProblemDetails'e dönüştürür.
    /// FluentValidation'ın <see cref="ValidationException"/>'ı 400'e (alan bazlı
    /// hatalarla), diğer her şey 500'e eşlenir. Program.cs'te AddExceptionHandler
    /// ile kaydedilir; UseExceptionHandler ile pipeline'a girer.
    /// </summary>
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            IProblemDetailsService problemDetailsService,
            ILogger<GlobalExceptionHandler> logger)
        {
            _problemDetailsService = problemDetailsService;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            ProblemDetails problemDetails = exception switch
            {
                ValidationException validationException => BuildValidationProblem(validationException),
                _ => BuildServerErrorProblem()
            };

            // Validation hataları beklenen akışın parçasıdır; gürültü yapmamak için Warning.
            if (exception is ValidationException)
            {
                _logger.LogWarning(exception, "Validation failed: {Message}", exception.Message);
            }
            else
            {
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
            }

            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            });
        }

        private static ProblemDetails BuildValidationProblem(ValidationException exception)
        {
            // Alan adı → hata mesajları sözlüğü; ProblemDetails.Extensions["errors"]'a konur.
            var errors = new Dictionary<string, string[]>();

            foreach (var failure in exception.Errors)
            {
                if (errors.TryGetValue(failure.PropertyName, out string[]? existing))
                {
                    var combined = new string[existing.Length + 1];
                    Array.Copy(existing, combined, existing.Length);
                    combined[existing.Length] = failure.ErrorMessage;
                    errors[failure.PropertyName] = combined;
                }
                else
                {
                    errors[failure.PropertyName] = new[] { failure.ErrorMessage };
                }
            }

            return new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Extensions = { ["errors"] = errors }
            };
        }

        private static ProblemDetails BuildServerErrorProblem() => new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
        };
    }
}
