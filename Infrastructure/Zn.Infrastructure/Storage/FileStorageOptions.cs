using System.Collections.Generic;

namespace Zn.Infrastructure.Storage
{
    /// <summary>
    /// Yerel dosya depolamanın yapılandırma ayarları. Varsayılanlar görsel yüklemeye uygundur;
    /// kök fiziksel yol (<see cref="RootPath"/>) Program.cs'te WebRootPath'e göre doldurulur.
    /// </summary>
    public sealed class FileStorageOptions
    {
        /// <summary>appsettings bağlama bölümü adı.</summary>
        public const string SectionName = "FileStorage";

        /// <summary>
        /// Dosyaların yazılacağı kök fiziksel klasör (örn. "{WebRoot}/uploads").
        /// Program.cs ortamın WebRootPath'ine göre çalışma zamanında ayarlar.
        /// </summary>
        public string RootPath { get; set; } = string.Empty;

        /// <summary>
        /// Dönen URL'in başına eklenen göreli temel yol (statik dosya request path'i ile eşleşmeli).
        /// </summary>
        public string RequestPath { get; set; } = "/uploads";

        /// <summary>İzin verilen azami dosya boyutu (bayt). Varsayılan 5 MB.</summary>
        public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;

        /// <summary>İzin verilen dosya uzantıları (küçük harf, nokta dahil).</summary>
        public IReadOnlyList<string> AllowedExtensions { get; set; } =
            new[] { ".jpg", ".jpeg", ".png", ".webp" };
    }
}
