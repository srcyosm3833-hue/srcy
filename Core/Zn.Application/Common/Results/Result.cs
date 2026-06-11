using System;

namespace Zn.Application.Common.Results
{
    /// <summary>
    /// Bir işlemin başarı/başarısızlık sonucunu, exception fırlatmadan taşıyan sarmalayıcı.
    /// Başarılıysa <see cref="IsSuccess"/> true ve <see cref="Error"/> = <see cref="Error.None"/>;
    /// başarısızsa <see cref="IsSuccess"/> false ve anlamlı bir <see cref="Error"/> taşınır.
    /// </summary>
    public class Result
    {
        protected Result(bool isSuccess, Error error)
        {
            // Tutarsız durumları erken yakala: başarılı sonuç hata taşıyamaz,
            // başarısız sonuç da hatasız olamaz.
            if (isSuccess && error != Error.None)
                throw new InvalidOperationException("A successful result cannot carry an error.");

            if (!isSuccess && error == Error.None)
                throw new InvalidOperationException("A failed result must carry an error.");

            IsSuccess = isSuccess;
            Error = error;
        }

        /// <summary>İşlemin başarılı olup olmadığı.</summary>
        public bool IsSuccess { get; }

        /// <summary>İşlemin başarısız olup olmadığı (kolaylık özelliği).</summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>Başarısızlık durumunda dolu olan hata; başarıda <see cref="Error.None"/>.</summary>
        public Error Error { get; }

        /// <summary>Başarılı (değer taşımayan) sonuç üretir.</summary>
        public static Result Success() => new(true, Error.None);

        /// <summary>Verilen hatayla başarısız sonuç üretir.</summary>
        public static Result Failure(Error error) => new(false, error);

        /// <summary>Başarılı, değer taşıyan sonuç üretir.</summary>
        public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);

        /// <summary>Belirli bir değer tipi için, verilen hatayla başarısız sonuç üretir.</summary>
        public static Result<TValue> Failure<TValue>(Error error) => Result<TValue>.Failure(error);
    }
}
