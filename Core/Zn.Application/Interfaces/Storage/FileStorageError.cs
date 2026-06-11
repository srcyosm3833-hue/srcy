namespace Zn.Application.Interfaces.Storage
{
    /// <summary>
    /// Dosya yükleme doğrulamasının başarısız olma nedeni. Sunum katmanı bunu uygun
    /// HTTP yanıtına (genellikle 400) eşler.
    /// </summary>
    public enum FileStorageError
    {
        /// <summary>Dosya boş (0 bayt) veya hiç gönderilmedi.</summary>
        Empty = 0,

        /// <summary>Dosya, izin verilen azami boyutu aşıyor.</summary>
        TooLarge = 1,

        /// <summary>Dosya uzantısı/MIME türü izin verilen listede değil.</summary>
        UnsupportedType = 2
    }
}
