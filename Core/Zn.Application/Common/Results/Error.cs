using System;
using System.Collections.Generic;

namespace Zn.Application.Common.Results
{
    /// <summary>
    /// Bir başarısızlığı tanımlayan değer nesnesi. Makinece okunabilir bir <see cref="Code"/>,
    /// insan tarafından okunabilir bir <see cref="Message"/> ve HTTP eşlemesini yönlendiren
    /// bir <see cref="Type"/> taşır. Validation hataları için alan bazlı detaylar
    /// <see cref="Validations"/> içinde tutulabilir.
    /// </summary>
    public sealed class Error : IEquatable<Error>
    {
        /// <summary>Hiçbir hatanın olmadığını temsil eden sentinel değer.</summary>
        public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

        private Error(string code, string message, ErrorType type, IReadOnlyDictionary<string, string[]>? validations = null)
        {
            Code = code;
            Message = message;
            Type = type;
            Validations = validations;
        }

        /// <summary>Makinece okunabilir hata kodu (örn. "User.NotFound").</summary>
        public string Code { get; }

        /// <summary>Kullanıcıya/loglamaya yönelik açıklama.</summary>
        public string Message { get; }

        /// <summary>HTTP durum koduna eşlenecek hata kategorisi.</summary>
        public ErrorType Type { get; }

        /// <summary>
        /// Validation hatalarında alan adı → mesajlar eşlemesi.
        /// ProblemDetails'in "errors" sözlüğüne doğrudan aktarılabilir.
        /// </summary>
        public IReadOnlyDictionary<string, string[]>? Validations { get; }

        /// <summary>Doğrulama (400) hatası üretir; isteğe bağlı alan bazlı detaylarla.</summary>
        public static Error Validation(string code, string message, IReadOnlyDictionary<string, string[]>? validations = null) =>
            new(code, message, ErrorType.Validation, validations);

        /// <summary>Bulunamadı (404) hatası üretir.</summary>
        public static Error NotFound(string code, string message) =>
            new(code, message, ErrorType.NotFound);

        /// <summary>Çakışma (409) hatası üretir.</summary>
        public static Error Conflict(string code, string message) =>
            new(code, message, ErrorType.Conflict);

        /// <summary>Kimlik doğrulama (401) hatası üretir.</summary>
        public static Error Unauthorized(string code, string message) =>
            new(code, message, ErrorType.Unauthorized);

        /// <summary>Yetkilendirme (403) hatası üretir.</summary>
        public static Error Forbidden(string code, string message) =>
            new(code, message, ErrorType.Forbidden);

        /// <summary>Kilitli kaynak (423) hatası üretir; örn. hesap kilidi.</summary>
        public static Error Locked(string code, string message) =>
            new(code, message, ErrorType.Locked);

        /// <summary>Genel sunucu (500) hatası üretir.</summary>
        public static Error Failure(string code, string message) =>
            new(code, message, ErrorType.Failure);

        public bool Equals(Error? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Code == other.Code && Type == other.Type;
        }

        public override bool Equals(object? obj) => Equals(obj as Error);

        public override int GetHashCode() => HashCode.Combine(Code, Type);

        public override string ToString() => $"{Type}: {Code} - {Message}";
    }
}
