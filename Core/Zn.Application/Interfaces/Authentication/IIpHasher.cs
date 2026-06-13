namespace Zn.Application.Interfaces.Authentication
{
    /// <summary>
    /// İstemci IP adreslerini, veritabanında saklamadan önce tek yönlü (tuzlu SHA-256) hash'leme
    /// sözleşmesi. İmplementasyon Zn.Infrastructure'da yer alır.
    /// <para>
    /// Tasarım notu (KVKK): Ham IP ASLA saklanmaz. IP kişisel veri olduğundan yalnızca tuzlu hash
    /// tutulur; böylece veri ihlalinde kişi doğrudan tanımlanamaz, ancak aynı IP'nin tekrar görünüp
    /// görünmediği (kötüye kullanım tespiti) korunur. Tuz <c>Audit:IpHashSalt</c> yapılandırmasından
    /// okunur ve refresh token hash'lemesinden (<see cref="ITokenHasher"/>) ayrı tutulur.
    /// </para>
    /// </summary>
    public interface IIpHasher
    {
        /// <summary>
        /// Verilen düz IP string'inin tuzlu, deterministik SHA-256 hash'ini (base64) üretir.
        /// Aynı (IP, tuz) çifti her zaman aynı çıktıyı verir; aynı IP'yi tespit edebilmek bunu
        /// gerektirir. <paramref name="ipAddress"/> null/boş ise null döner (audit opsiyoneldir).
        /// </summary>
        /// <param name="ipAddress">Hash'lenecek düz istemci IP adresi; null/boş olabilir.</param>
        /// <returns>Base64 kodlanmış tuzlu SHA-256 hash; girdi null/boş ise null.</returns>
        string? Hash(string? ipAddress);
    }
}
