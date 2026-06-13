using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zn.Application.Common.Pagination;
using Zn.Application.Common.Results;
using Zn.Application.Features.Blogs.Common;
using Zn.Application.Interfaces.Audit;
using Zn.Application.Interfaces.Authentication;
using Zn.Application.Interfaces.Persistence;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Blogs.Search
{
    /// <summary>
    /// <see cref="SearchBlogsQuery"/>'i işleyen Wolverine handler'ı. Query doğrulaması
    /// (Q zorunlu, uzunluk + pageSize aralığı) Wolverine FluentValidation middleware'i
    /// tarafından handler'dan önce uygulanır (<see cref="SearchBlogsQueryValidator"/>); bu
    /// sayede boş/geçersiz terim handler'a hiç ulaşmaz ve log YAZILMAZ.
    /// Burada page güvenli alt sınıra çekilir, repository'den DB seviyesinde projekte edilmiş
    /// sayfa alınır ve Mapperly ile yanıta dönüştürülüp <see cref="PagedResult{T}"/> olarak
    /// sarılır. Sonuç boş olabilir (her zaman Success).
    /// <para>
    /// Yan etki (audit): Her başarılı aramada bir <see cref="SearchLog"/> kaydı yazılır
    /// (terim, kullanıcı kimliği + ad-soyad snapshot'ı, hash'li IP, zaman). Log yazımı asıl
    /// aramayı ASLA bloklamaz: tüm loglama try/catch ile sarılır ve hata yalnızca yapılandırılmış
    /// log olarak kaydedilir (A-AU6 kararı).
    /// </para>
    /// </summary>
    public static class SearchBlogsQueryHandler
    {
        public static async Task<Result<PagedResult<BlogListItemResponse>>> Handle(
            SearchBlogsQuery query,
            IBlogRepository blogRepository,
            ISearchLogRepository searchLogRepository,
            IUserRepository userRepository,
            IClientIpResolver clientIpResolver,
            IIpHasher ipHasher,
            ILogger<SearchBlogsQueryResult> logger,
            CancellationToken cancellationToken)
        {
            // Validator pageSize'ı 1–MaxPageSize aralığında garanti eder; page için yalnızca
            // alt sınırı (≥ 1) defansif olarak normalize ediyoruz.
            int page = query.Page < 1 ? 1 : query.Page;
            int pageSize = query.PageSize;

            // Public arama: silinmiş bloglar global query filter ile zaten dışlanır.
            (IReadOnlyList<BlogListItem> items, int totalCount) =
                await blogRepository.SearchAsync(
                    query.Q, query.CategoryId, page, pageSize, query.CurrentUserId, cancellationToken);

            IReadOnlyList<BlogListItemResponse> mapped = BlogMapper.ToListItemResponseList(items);

            var pagedResult = new PagedResult<BlogListItemResponse>(mapped, totalCount, page, pageSize);

            // Audit logu: asıl aramayı bloklamaması için ayrı, hata-yutan bir yöntemde yazılır.
            await TryWriteSearchLogAsync(
                query, searchLogRepository, userRepository, clientIpResolver, ipHasher, logger, cancellationToken);

            return Result.Success(pagedResult);
        }

        /// <summary>
        /// Arama log kaydını yazar. Hiçbir koşulda istisna sızdırmaz: terim çözümü, kullanıcı adı
        /// çözümü, IP hash'leme veya DB yazımı başarısız olursa hata yalnızca yapılandırılmış log
        /// olarak kaydedilir ve arama akışı etkilenmez (A-AU6).
        /// </summary>
        private static async Task TryWriteSearchLogAsync(
            SearchBlogsQuery query,
            ISearchLogRepository searchLogRepository,
            IUserRepository userRepository,
            IClientIpResolver clientIpResolver,
            IIpHasher ipHasher,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            try
            {
                // IP'yi çöz ve sakla(ma)dan önce hash'le; çözülemezse null (audit opsiyonel).
                string? ipHash = ipHasher.Hash(clientIpResolver.ResolveIpAddress());

                // "Kim aradı" snapshot'ı: giriş yapılmışsa log anındaki ad-soyad DB'den okunur.
                string? userFullName = null;
                if (!string.IsNullOrWhiteSpace(query.CurrentUserId))
                {
                    userFullName =
                        await userRepository.GetFullNameByIdAsync(query.CurrentUserId, cancellationToken);
                }

                SearchLog log = SearchLog.Create(query.Q, query.CurrentUserId, userFullName, ipHash);

                await searchLogRepository.AddAsync(log, cancellationToken);
            }
            catch (Exception ex)
            {
                // Loglama hatası aramayı bozmaz; yalnızca yapılandırılmış log bırakılır.
                logger.LogWarning(ex, "Search audit log could not be written for term '{Term}'.", query.Q);
            }
        }
    }

    /// <summary>
    /// <see cref="ILogger{TCategoryName}"/> kategori adı için marker tip. Handler statik olduğundan
    /// kendi tipi üzerinden ILogger alınamaz; bu marker, log kategorisini anlamlı kılar.
    /// </summary>
    public sealed class SearchBlogsQueryResult
    {
    }
}
