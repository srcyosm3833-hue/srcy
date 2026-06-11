using System;

namespace Zn.Application.Common.Results
{
    /// <summary>
    /// Başarı durumunda bir <typeparamref name="TValue"/> taşıyan <see cref="Result"/> türevi.
    /// Başarısız bir sonuçta <see cref="Value"/>'ya erişmek exception fırlatır;
    /// erişimden önce <see cref="Result.IsSuccess"/> kontrol edilmelidir.
    /// </summary>
    /// <typeparam name="TValue">Başarı durumunda dönen değerin tipi.</typeparam>
    public sealed class Result<TValue> : Result
    {
        private readonly TValue? _value;

        private Result(TValue? value, bool isSuccess, Error error)
            : base(isSuccess, error)
        {
            _value = value;
        }

        /// <summary>
        /// Başarı durumundaki değer. Sonuç başarısızsa erişim
        /// <see cref="InvalidOperationException"/> fırlatır.
        /// </summary>
        public TValue Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("The value of a failed result cannot be accessed.");

        /// <summary>Verilen değerle başarılı sonuç üretir.</summary>
        public static Result<TValue> Success(TValue value) => new(value, true, Error.None);

        /// <summary>Verilen hatayla başarısız sonuç üretir.</summary>
        public static new Result<TValue> Failure(Error error) => new(default, false, error);

        /// <summary>
        /// Bir değerden örtük olarak başarılı sonuç üretir.
        /// null değer için Failure üretmek çağıranın sorumluluğundadır.
        /// </summary>
        public static implicit operator Result<TValue>(TValue value) => Success(value);
    }
}
