namespace Zn.Domain.Exceptions
{
    /// <summary>
    /// SubComment entity'sinin invariant'larından biri ihlal edildiğinde fırlatılır
    /// (örn. boş alt yorum metni veya azami uzunluğun aşılması, geçersiz yorum/yazar referansı).
    /// <see cref="Zn.Domain.Entity.SubComment"/>'in factory/mutator metotları tarafından atılır;
    /// böylece geçersiz bir SubComment nesnesi hiçbir zaman var olamaz.
    /// </summary>
    public sealed class SubCommentDomainException : DomainException
    {
        public SubCommentDomainException(string message) : base(message)
        {
        }
    }
}
