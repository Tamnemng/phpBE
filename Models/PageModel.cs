using System;
using System.Collections.Generic;
using System.Linq;

namespace OMS.Core.Queries
{
    public sealed class PagedModel<T>
    {
        public const int DefaultItemsPerPage = 20;

        public long TotalCount { get; }

        public IEnumerable<T> Data { get; }

        public int PageIndex { get; }

        public int PageSize { get; }

        public PagedModel(long totalCount, IEnumerable<T> data, int pageIndex, int pageSize)
        {
            TotalCount = totalCount;
            Data = data;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }

        public static PagedModel<T> Empty => new PagedModel<T>(0, Enumerable.Empty<T>(), 0, DefaultItemsPerPage);

        public object ToList()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class PagedModel<T, U>
    {
        public const int DefaultItemsPerPage = 20;

        public long TotalCount { get; }

        public IEnumerable<T> Data { get; }

        public int PageIndex { get; }

        public int PageSize { get; }


        public PagedModel(long totalCount, IEnumerable<T> data, int pageIndex, int pageSize)
        {
            TotalCount = totalCount;
            Data = data;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }
    }
}
