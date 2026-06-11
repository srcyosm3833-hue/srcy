namespace Zn.Domain.Exceptions
{
    /// <summary>
    /// SocialMedia entity'sinin invariant'larından biri ihlal edildiğinde fırlatılır
    /// (örn. boş Title/Url/Icon veya azami uzunluğun aşılması). <see cref="Entity.SocialMedia"/>'nın
    /// factory/mutator metotları tarafından atılır.
    /// </summary>
    public sealed class SocialMediaDomainException : DomainException
    {
        public SocialMediaDomainException(string message) : base(message)
        {
        }
    }
}
