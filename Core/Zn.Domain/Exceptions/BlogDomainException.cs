namespace Zn.Domain.Exceptions
{
    /// <summary>
    /// Blog entity'sinin invariant'larından biri ihlal edildiğinde fırlatılır
    /// (örn. boş başlık/açıklama/görsel, azami uzunluğun aşılması veya geçersiz
    /// kategori/yazar referansı). <see cref="Zn.Domain.Entity.Blog"/>'un factory/mutator
    /// metotları tarafından atılır; böylece geçersiz bir Blog nesnesi hiçbir zaman var olamaz.
    /// </summary>
    public sealed class BlogDomainException : DomainException
    {
        public BlogDomainException(string message) : base(message)
        {
        }
    }
}
