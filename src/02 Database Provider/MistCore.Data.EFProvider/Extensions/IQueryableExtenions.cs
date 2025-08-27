using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MistCore.Data
{
    /// <summary>
    /// IQueryableExtenions
    /// </summary>
    public static class IQueryableExtenions
    {

        /// <summary>
        /// To List Async.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> queryable)
        {
            return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(queryable);
            //return await queryable.ToListAsync();
        }

        /// <summary>
        /// To SQL.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToSql<T>(this IQueryable<T> source)
        {
            var sql = source.ToQueryString();
            return sql;
        }

    }
}
