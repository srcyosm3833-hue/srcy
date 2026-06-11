namespace Zn.Domain.Exceptions
{
    /// <summary>
    /// Comment entity'sinin invariant'larından biri ihlal edildiğinde fırlatılır
    /// (örn. boş yorum metni veya azami uzunluğun aşılması, geçersiz blog/yazar referansı).
    /// <see cref="Zn.Domain.Entity.Comment"/>'in factory/mutator metotları tarafından atılır;
    /// böylece geçersiz bir Comment nesnesi hiçbir zaman var olamaz.
    /// </summary>
    public sealed class CommentDomainException : DomainException
    {
        public CommentDomainException(string message) : base(message)
        {
        }
    }
}
