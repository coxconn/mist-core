using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MistCore.Data
{
    /// <summary>
    /// Pagination extensions
    /// </summary>
    public static class PaginationExtensions
    {
        private const int MAX_PAGE_SIZE = 100;
        private const int MIN_PAGE_SIZE = 0;

        /// <summary>
        /// Paginations the specified source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="pageInfo">The pageinfo.</param>
        /// <returns></returns>
        public static IQueryable<T> Pagination<T>(this IQueryable<T> source, PageInfo pageInfo)
        {
            if (pageInfo.Take > 0)
            {
                return source.Skip(pageInfo.Skip).Take(pageInfo.Take);
            }

            if (pageInfo.PageNo == -1)
            {
                return source;
            }
            pageInfo.Total = source.LongCount();
            pageInfo.PageSize = Math.Min(Math.Max(MIN_PAGE_SIZE, pageInfo.PageSize), MAX_PAGE_SIZE);
            pageInfo.PageCount = (int)Math.Ceiling(pageInfo.Total / (double)pageInfo.PageSize);
            //pageInfo.PageNo = (pageInfo.PageNo <= 0 || pageInfo.PageNo > pageInfo.PageCount) ? 1 : pageInfo.PageNo;
            pageInfo.PageNo = pageInfo.PageNo <= 0 ? 1 : pageInfo.PageNo;
            return source.Skip((pageInfo.PageNo - 1) * pageInfo.PageSize).Take(pageInfo.PageSize);
        }

        /// <summary>
        /// Paginations the specified page no.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="pageNo">The page no.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageCount">The page count.</param>
        /// <param name="totalSize">The total size.</param>
        /// <returns></returns>
        public static IQueryable<T> Pagination<T>(this IQueryable<T> source, ref int pageNo, ref int pageSize, ref int pageCount, ref long totalSize)
        {
            if (pageNo == -1)
            {
                return source;
            }
            totalSize = source.LongCount();
            pageSize = pageSize <= 0 ? 10 : pageSize;
            pageSize = Math.Min(Math.Max(MIN_PAGE_SIZE, pageSize), MAX_PAGE_SIZE);
            pageCount = (int)Math.Ceiling(totalSize / (double)pageSize);
            //pageNo = (pageNo <= 0 || pageNo > pageCount) ? 1 : pageNo;
            pageNo = pageNo <= 0 ? 1 : pageNo;
            return source.Skip((pageNo - 1) * pageSize).Take(pageSize);
        }

    }
}
