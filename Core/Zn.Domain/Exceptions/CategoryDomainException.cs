namespace Zn.Domain.Exceptions
{
    /// <summary>
    /// Category entity'sinin invariant'larından biri ihlal edildiğinde fırlatılır
    /// (örn. boş kategori adı veya azami uzunluğun aşılması). <see cref="Category"/>'nin
    /// factory/mutator metotları tarafından atılır.
    /// </summary>
    public sealed class CategoryDomainException : DomainException
    {
        public CategoryDomainException(string message) : base(message)
        {
        }
    }
}
