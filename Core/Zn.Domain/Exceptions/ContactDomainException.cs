namespace Zn.Domain.Exceptions
{
    /// <summary>
    /// Contact entity'sinin invariant'larından biri ihlal edildiğinde fırlatılır
    /// (örn. boş adres/e-posta/telefon/harita URL'i veya azami uzunlukların aşılması).
    /// <see cref="Zn.Domain.Entity.Contact"/>'in factory/mutator metotları tarafından atılır;
    /// böylece geçersiz bir Contact nesnesi hiçbir zaman var olamaz.
    /// </summary>
    public sealed class ContactDomainException : DomainException
    {
        public ContactDomainException(string message) : base(message)
        {
        }
    }
}
