namespace Zn.Domain.Exceptions
{
    /// <summary>
    /// SearchLog entity'sinin invariant'larından biri ihlal edildiğinde fırlatılır
    /// (örn. boş arama terimi veya azami uzunlukların aşılması).
    /// <see cref="Zn.Domain.Entity.SearchLog"/>'un factory metodu tarafından atılır;
    /// böylece geçersiz bir SearchLog nesnesi hiçbir zaman var olamaz.
    /// </summary>
    public sealed class SearchLogDomainException : DomainException
    {
        public SearchLogDomainException(string message) : base(message)
        {
        }
    }
}
