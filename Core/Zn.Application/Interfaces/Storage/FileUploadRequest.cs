using System.IO;

namespace Zn.Application.Interfaces.Storage
{
    /// <summary>
    /// Depolama katmanına yüklenecek dosyayı temsil eden, framework'ten bağımsız istek nesnesi.
    /// Sunum katmanı (controller) <c>IFormFile</c>'ı bu tipe çevirir; böylece Application/Infrastructure
    /// ASP.NET'in IFormFile tipine doğrudan bağımlı olmaz (Onion disiplini).
    /// </summary>
    public sealed class FileUploadRequest
    {
        public FileUploadRequest(Stream content, string fileName, string contentType, long length)
        {
            Content = content;
            FileName = fileName;
            ContentType = contentType;
            Length = length;
        }

        /// <summary>Dosya içeriğinin okunabilir akışı.</summary>
        public Stream Content { get; }

        /// <summary>İstemcinin gönderdiği orijinal dosya adı (uzantı çıkarımı için kullanılır).</summary>
        public string FileName { get; }

        /// <summary>İstemcinin bildirdiği MIME türü.</summary>
        public string ContentType { get; }

        /// <summary>Dosya boyutu (bayt). Boyut sınırı doğrulaması için kullanılır.</summary>
        public long Length { get; }
    }
}
