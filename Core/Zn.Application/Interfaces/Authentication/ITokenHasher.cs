namespace Zn.Application.Interfaces.Authentication
{
    /// <summary>
    /// Refresh token'ları veritabanında saklamadan önce tek yönlü hash'leme sözleşmesi.
    /// İmplementasyon Zn.Infrastructure'da yer alır.
    /// <para>
    /// Tasarım notu: Düz refresh token yalnızca istemciye döner; veritabanında yalnızca
    /// hash saklanır. Böylece DB sızıntısında token'lar doğrudan kullanılamaz. Refresh
    /// token zaten kriptografik olarak güçlü (64 bayt rastgele) olduğundan, parola gibi
    /// salt + yavaş hash'e gerek yoktur; deterministik SHA-256 yeterlidir ve DB'de
    /// hash ile birebir arama (unique index) yapılabilmesini sağlar.
    /// </para>
    /// </summary>
    public interface ITokenHasher
    {
        /// <summary>
        /// Verilen düz token string'inin deterministik SHA-256 hash'ini (base64) üretir.
        /// Aynı girdi her zaman aynı çıktıyı verir; DB'de hash bazlı arama bunu gerektirir.
        /// </summary>
        /// <param name="token">İstemciye dönen düz refresh token string'i.</param>
        /// <returns>Base64 kodlanmış SHA-256 hash.</returns>
        string Hash(string token);
    }
}
