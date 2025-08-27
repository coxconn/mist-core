using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Linq
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> OrderByName<T>(this IQueryable<T> source, List<string> orders)
        {
            if (source == null)
            {
                throw new ArgumentException(nameof(source));
            }

            IQueryable<T> query = source;

            foreach (var order in orders)
            {
                var orderby = (order ?? string.Empty).Split(',');

                string propertyName = orderby[0];
                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    throw new ArgumentException(nameof(propertyName));
                }
                bool isDescending = orderby.Length > 1 ? "desc".Equals(orderby[1]) : false;

                if (query == source)
                {
                    query = OrderByName<T>(query, propertyName, isDescending);
                }
                else
                {
                    query = ThenByName<T>(query, propertyName, isDescending);
                }
            }

            return query;
        }

        public static IQueryable<T> OrderByName<T>(this IQueryable<T> source, string propertyName, bool isDescending)
        {
            if (source == null)
            {
                throw new ArgumentException(nameof(source));
            }

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException(nameof(propertyName));
            }

            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            PropertyInfo propertyInfo = type.GetProperty(propertyName);
            Expression expression = Expression.Property(arg, propertyInfo);
            type = propertyInfo.PropertyType;

            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expression, arg);

            var methodName = isDescending ? "OrderByDescending" : "OrderBy";
            object result = typeof(Queryable).GetMethods().Single(
                method => method.Name == methodName
                        && method.IsGenericMethodDefinition
                        && method.GetGenericArguments().Length == 2
                        && method.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), type)
                .Invoke(null, new object[] { source, lambda });
            return (IQueryable<T>)result;
        }

        public static IQueryable<T> ThenByName<T>(this IQueryable<T> source, string propertyName, bool isDescending)
        {
            if (source == null)
            {
                throw new ArgumentException(nameof(source));
            }

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException(nameof(propertyName));
            }

            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            PropertyInfo propertyInfo = type.GetProperty(propertyName);
            Expression expression = Expression.Property(arg, propertyInfo);
            type = propertyInfo.PropertyType;

            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expression, arg);

            var methodName = isDescending ? "ThenByDescending" : "ThenBy";
            object result = typeof(Queryable).GetMethods().Single(
                method => method.Name == methodName
                        && method.IsGenericMethodDefinition
                        && method.GetGenericArguments().Length == 2
                        && method.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), type)
                .Invoke(null, new object[] { source, lambda });
            return (IQueryable<T>)result;
        }

    }
}
