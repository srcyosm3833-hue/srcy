namespace Zn.Domain.Exceptions
{
    /// <summary>
    /// Message entity'sinin invariant'larından biri ihlal edildiğinde fırlatılır
    /// (örn. boş gönderen adı/e-posta/konu/mesaj gövdesi veya azami uzunlukların aşılması).
    /// <see cref="Zn.Domain.Entity.Message"/>'in factory/mutator metotları tarafından atılır;
    /// böylece geçersiz bir Message nesnesi hiçbir zaman var olamaz.
    /// </summary>
    public sealed class MessageDomainException : DomainException
    {
        public MessageDomainException(string message) : base(message)
        {
        }
    }
}
