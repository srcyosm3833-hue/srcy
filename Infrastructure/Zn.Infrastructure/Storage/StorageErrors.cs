using Zn.Application.Common.Results;

namespace Zn.Infrastructure.Storage
{
    /// <summary>
    /// Dosya depolama doğrulama hatalarının anlamlı <see cref="Error"/> fabrikalarını toplar.
    /// Tümü Validation (400) tipindedir; ApiControllerBase bunları 400 ProblemDetails'e eşler.
    /// </summary>
    internal static class StorageErrors
    {
        public static Error Empty() =>
            Error.Validation("Upload.Empty", "The uploaded file is empty.");

        public static Error TooLarge(long maxBytes) =>
            Error.Validation(
                "Upload.TooLarge",
                $"The uploaded file exceeds the maximum allowed size of {maxBytes / (1024 * 1024)} MB.");

        public static Error UnsupportedType(string allowed) =>
            Error.Validation(
                "Upload.UnsupportedType",
                $"Unsupported file type. Allowed extensions: {allowed}.");
    }
}
