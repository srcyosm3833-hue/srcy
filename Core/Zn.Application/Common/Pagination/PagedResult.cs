using System;
using System.Collections.Generic;

namespace Zn.Application.Common.Pagination
{
    /// <summary>
    /// Sayfalanmış sorgu sonucunu temsil eder: geçerli sayfadaki öğeler
    /// ve sayfalama meta verisi (toplam kayıt, sayfa no, sayfa boyutu, toplam sayfa).
    /// </summary>
    /// <typeparam name="T">Listelenen öğe tipi (genellikle bir DTO).</typeparam>
    public sealed class PagedResult<T>
    {
        public PagedResult(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }

        /// <summary>Geçerli sayfadaki öğeler.</summary>
        public IReadOnlyList<T> Items { get; }

        /// <summary>Tüm sayfalardaki toplam kayıt sayısı.</summary>
        public int TotalCount { get; }

        /// <summary>1 tabanlı geçerli sayfa numarası.</summary>
        public int Page { get; }

        /// <summary>Sayfa başına öğe sayısı.</summary>
        public int PageSize { get; }

        /// <summary>Toplam sayfa sayısı; <see cref="PageSize"/> 0 ise 0 döner.</summary>
        public int TotalPages => PageSize <= 0
            ? 0
            : (int)Math.Ceiling(TotalCount / (double)PageSize);

        /// <summary>Bir önceki sayfanın olup olmadığı.</summary>
        public bool HasPreviousPage => Page > 1;

        /// <summary>Bir sonraki sayfanın olup olmadığı.</summary>
        public bool HasNextPage => Page < TotalPages;
    }
}
