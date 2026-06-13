namespace Zn.Application.Interfaces.Audit
{
    /// <summary>
    /// Geçerli HTTP isteğinden istemci IP adresini çözen sözleşme. İmplementasyon, ASP.NET
    /// <c>IHttpContextAccessor</c>'a erişimi olan Presentation/Infrastructure katmanında yer alır;
    /// böylece Application handler'ları HTTP altyapısına doğrudan bağımlı olmadan IP'yi elde eder.
    /// <para>
    /// Çözümleme önceliği: önce <c>X-Forwarded-For</c> başlığı (proxy/CDN arkasında gerçek istemci),
    /// fallback olarak <c>RemoteIpAddress</c>. IP çözümlenemezse (örn. test ortamı, HttpContext yok)
    /// null döner ve ASLA hata fırlatmaz — audit opsiyoneldir, asıl iş akışını bloklamaz.
    /// </para>
    /// </summary>
    public interface IClientIpResolver
    {
        /// <summary>
        /// Geçerli isteğin istemci IP adresini döner; çözümlenemezse null. Hata fırlatmaz.
        /// Dönen değer HAM IP'dir; saklamadan önce <see cref="Authentication.IIpHasher"/> ile
        /// hash'lenmelidir.
        /// </summary>
        string? ResolveIpAddress();
    }
}
