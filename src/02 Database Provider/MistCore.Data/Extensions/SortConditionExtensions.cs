using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace MistCore.Data
{
    /// <summary>
    /// Sort condition extensions
    /// </summary>
    public static class SortConditionExtensions
    {
        /// <summary>
        /// Tries the order by.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="pageInfo">The page information.</param>
        /// <returns></returns>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, PageInfo pageInfo)
        {
            #region Order
            if (!string.IsNullOrWhiteSpace(pageInfo.Order))
            {
                var orders = (pageInfo.Order ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c =>
                    {
                        var orderby = (c ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        string propertyName = orderby[0];
                        if (string.IsNullOrWhiteSpace(propertyName))
                        {
                            throw new ArgumentException(nameof(propertyName));
                        }
                        bool isDescending = orderby.Length > 1 ? "desc".Equals(orderby[1]) : false;
                        return new
                        {
                            propertyName = propertyName,
                            isDescending = isDescending,
                        };
                    })
                    .ToList();

                if (orders.Count == 0)
                {
                    return source;
                }

                IOrderedQueryable<T> query = orders[0].isDescending ? source.OrderByDescending(orders[0].propertyName) : source.OrderBy(orders[0].propertyName);

                orders.Skip(1).ToList().ForEach(c =>
                {
                    query = c.isDescending ? query.ThenByDescending(c.propertyName) : query.ThenBy(c.propertyName);
                });

                return query;
            }
            #endregion

            return source;

            //#region Sort
            //if (string.IsNullOrEmpty(pageInfo.sort))
            //{
            //    pageInfo.sort = typeof(T).GetProperties()[0].Name;
            //}

            //if (pageInfo.isDesc == true)
            //{
            //    return source.OrderByDescending(pageInfo.sort);
            //}
            //else
            //{
            //    return source.OrderBy(pageInfo.sort);
            //}
            //#endregion
        }

        /// <summary>
        /// Orders the by.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            return QueryableHelper.OrderBy(source, propertyName);
        }

        /// <summary>
        /// Orders the by descending.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName)
        {
            return QueryableHelper.OrderByDescending(source, propertyName);
        }

        /// <summary>
        /// Orders the by.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string propertyName)
        {
            return QueryableHelper.ThenBy(source, propertyName);
        }

        /// <summary>
        /// Orders the by descending.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string propertyName)
        {
            return QueryableHelper.ThenByDescending(source, propertyName);
        }

        #region Queryable Helper
        /// <summary>
        /// Queryable helper
        /// </summary>
        static class QueryableHelper
        {
            /// <summary>
            /// LambdaExpression cache
            /// </summary>
            private static ConcurrentDictionary<string, LambdaExpression> cache = new ConcurrentDictionary<string, LambdaExpression>();

            /// <summary>
            /// Orders the by.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="queryable">The queryable.</param>
            /// <param name="propertyName">Name of the property.</param>
            /// <param name="isCache">if set to <c>true</c> [is cache].</param>
            /// <returns></returns>
            public static IOrderedQueryable<T> OrderBy<T>(IQueryable<T> queryable, string propertyName, bool isCache = true)
            {
                dynamic keySelector = isCache ? GetCacheLambdaExpression<T>(propertyName) : GetLambdaExpression<T>(propertyName);
                //return queryable.OrderBy(keySelector);
                return Queryable.OrderBy(queryable, keySelector);
            }

            /// <summary>
            /// Orders the by descending.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="queryable">The queryable.</param>
            /// <param name="propertyName">Name of the property.</param>
            /// <param name="isCache">if set to <c>true</c> [is cache].</param>
            /// <returns></returns>
            public static IOrderedQueryable<T> OrderByDescending<T>(IQueryable<T> queryable, string propertyName, bool isCache = true)
            {
                dynamic keySelector = isCache ? GetCacheLambdaExpression<T>(propertyName) : GetLambdaExpression<T>(propertyName);
                return Queryable.OrderByDescending(queryable, keySelector);
            }

            /// <summary>
            /// Then the by.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="queryable">The queryable.</param>
            /// <param name="propertyName">Name of the property.</param>
            /// <param name="isCache">if set to <c>true</c> [is cache].</param>
            /// <returns></returns>
            public static IOrderedQueryable<T> ThenBy<T>(IOrderedQueryable<T> queryable, string propertyName, bool isCache = true)
            {
                dynamic keySelector = isCache ? GetCacheLambdaExpression<T>(propertyName) : GetLambdaExpression<T>(propertyName);
                //return queryable.OrderBy(keySelector);
                return Queryable.ThenBy(queryable, keySelector);
            }

            /// <summary>
            /// Then the by descending.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="queryable">The queryable.</param>
            /// <param name="propertyName">Name of the property.</param>
            /// <param name="isCache">if set to <c>true</c> [is cache].</param>
            /// <returns></returns>
            public static IOrderedQueryable<T> ThenByDescending<T>(IOrderedQueryable<T> queryable, string propertyName, bool isCache = true)
            {
                dynamic keySelector = isCache ? GetCacheLambdaExpression<T>(propertyName) : GetLambdaExpression<T>(propertyName);
                return Queryable.ThenByDescending(queryable, keySelector);
            }

            /// <summary>
            /// Gets the cache lambda expression.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="propertyName">Name of the property.</param>
            /// <returns></returns>
            /// <exception cref="System.ArgumentNullException"></exception>
            private static LambdaExpression GetCacheLambdaExpression<T>(String propertyName)
            {
                if (string.IsNullOrEmpty(propertyName))
                    throw new ArgumentNullException(propertyName);
                string sKey = string.Format("@{0}._{1}", typeof(T).Name, propertyName);
                if (cache.ContainsKey(sKey))
                    return cache[sKey];
                LambdaExpression keySelector = GetLambdaExpression<T>(propertyName);
                cache[sKey] = keySelector;
                return keySelector;
            }

            /// <summary>
            /// Gets the lambda expression.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="propertyName">Name of the property.</param>
            /// <returns></returns>
            private static LambdaExpression GetLambdaExpression<T>(String propertyName)
            {
                ParameterExpression param = Expression.Parameter(typeof(T));
                Expression body = param;
                String[] propertyStrs = propertyName.Split('.');
                foreach (String propertyStr in propertyStrs)
                {
                    body = Expression.Property(body, propertyStr);
                }
                LambdaExpression keySelector = Expression.Lambda(body, param);
                return keySelector;

                //ParameterExpression arg = Expression.Parameter(typeof(T));
                //PropertyInfo property = type.GetProperty(propertyName);
                //Expression expr = Expression.Property(arg, propertyName);
                //Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), property.PropertyType);
                //LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);
                //return ((IOrderedQueryable<T>)(typeof(Queryable).GetMethods().Single(p => String.Equals(p.Name, methodName, StringComparison.Ordinal)
                //    && p.IsGenericMethodDefinition
                //    && p.GetGenericArguments().Length == 2
                //    && p.GetParameters().Length == 2)
                //    //.MakeGenericMethod(typeof(T), property.PropertyType)
                //    .Invoke(null, new Object[] { source, lambda })));
            }
        }
        #endregion
    }
}
