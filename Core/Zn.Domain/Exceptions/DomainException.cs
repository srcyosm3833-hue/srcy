using System;

namespace Zn.Domain.Exceptions
{
    /// <summary>
    /// Domain invariant'larının (iş kuralı değişmezlerinin) ihlal edildiği durumlarda
    /// fırlatılan temel exception tipi. Entity'lerin factory/constructor'ları geçersiz
    /// bir durum tespit ettiğinde bu tipten türeyen bir exception atar; böylece geçersiz
    /// bir entity hiçbir zaman oluşturulamaz.
    /// <para>
    /// Application katmanı, command/query'leri FluentValidation ile önceden doğruladığı
    /// için bu exception'lar normal akışta tetiklenmez; son savunma hattı olarak vardır.
    /// </para>
    /// </summary>
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message)
        {
        }
    }
}
