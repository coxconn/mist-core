using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;

namespace MistCore.Framework.Cached.RedisProvider
{
    public static class RedisExtensions
    {

        #region ConvertRedisValueToObject
        /// <summary>
        /// 将 RedisValue 转换为指定类型的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ToObject<T>(this RedisValue value)
        {
            if (value.IsNull)
                return default;
            var targetType = typeof(T);
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (underlyingType == typeof(string))
            {
                return (T)(object)value.ToString();
            }
            else if (underlyingType == typeof(int))
            {
                return (T)(object)(int)value;
            }
            else if (underlyingType == typeof(long))
            {
                return (T)(object)(long)value;
            }
            else if (underlyingType == typeof(double))
            {
                return (T)(object)(double)value;
            }
            else if (underlyingType == typeof(float))
            {
                return (T)(object)(float)value;
            }
            else if (underlyingType == typeof(decimal))
            {
                return (T)(object)(decimal)value;
            }
            else if (underlyingType == typeof(uint))
            {
                return (T)(object)(uint)value;
            }
            else if (underlyingType == typeof(ulong))
            {
                return (T)(object)(ulong)value;
            }
            else if (underlyingType == typeof(bool))
            {
                return (T)(object)(bool)value;
            }
            else if (underlyingType == typeof(byte[]))
            {
                return (T)(object)(byte[])value;
            }
            else if (underlyingType == typeof(Memory<byte>))
            {
                return (T)(object)(Memory<byte>)(byte[])value;
            }
            else if (underlyingType == typeof(ReadOnlyMemory<byte>))
            {
                return (T)(object)(ReadOnlyMemory<byte>)(byte[])value;
            }
            else if (underlyingType == typeof(short))
            {
                return (T)(object)Convert.ToInt16(value.ToString());
            }
            else if (underlyingType == typeof(DateTime))
            {
                return (T)(object)DateTime.Parse(value.ToString());
            }
            else
            {
                // 处理复杂类型（JSON 反序列化）
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value.ToString());
            }
        }
        #endregion

        #region ConvertObjectToRedisValue
        public static RedisValue ToRedisValue(object obj)
        {
            if (obj == null)
                return RedisValue.Null;
            var type = obj.GetType();
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            if (underlyingType == typeof(string) ||
                   underlyingType == typeof(int) || underlyingType == typeof(int?) ||
                   underlyingType == typeof(long) || underlyingType == typeof(long?) ||
                   underlyingType == typeof(double) || underlyingType == typeof(double?) ||
                   underlyingType == typeof(float) || underlyingType == typeof(float?) ||
                   underlyingType == typeof(decimal) || underlyingType == typeof(decimal?) ||
                   underlyingType == typeof(uint) || underlyingType == typeof(uint?) ||
                   underlyingType == typeof(ulong) || underlyingType == typeof(ulong?) ||
                   underlyingType == typeof(bool) || underlyingType == typeof(bool?) ||
                   underlyingType == typeof(byte[]) ||
                   underlyingType == typeof(Memory<byte>) ||
                   underlyingType == typeof(ReadOnlyMemory<byte>)) // 检查是否支持隐式转换
            {
                return (RedisValue)obj;
            }
            else if (underlyingType == typeof(short))
            {
                return (RedisValue)(int)(short)obj;
            }
            else if (underlyingType == typeof(DateTime))
            {
                return (RedisValue)((DateTime)obj).ToString("o"); // 使用 ISO 8601 格式
            }
            else if (underlyingType.IsPrimitive)
            {
                return (RedisValue)obj.ToString();
            }
            else
            {
                // 不支持隐式转换的类型，需要序列化
                return (RedisValue)Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            }
        }
        #endregion

        #region ConvertHashEntriesToObject

        private static readonly ConcurrentDictionary<Type, Func<HashEntry[], object>> _hashEntryConverters = new ConcurrentDictionary<Type, Func<HashEntry[], object>>();

        /// <summary>
        /// 将 HashEntry 数组转换为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashEntries"></param>
        /// <returns></returns>
        public static T ToObject<T>(this HashEntry[] hashEntries) where T : new()
        {
            if (hashEntries == null || hashEntries.Length == 0)
                return new T();

            var converter = _hashEntryConverters.GetOrAdd(typeof(T), CreateHashEntryConverterSimple<T>());
            return (T)converter(hashEntries);
        }

        /// <summary>
        /// HashEntry 到对象的转换器
        /// </summary>
        private static Func<HashEntry[], object> CreateHashEntryConverterSimple<T>() where T : new()
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite &&
                           !Attribute.IsDefined(p, typeof(RedisIgnoreAttribute)))
                .ToArray();

            var expressions = new List<Expression>();

            var hashEntriesParam = Expression.Parameter(typeof(HashEntry[]), "hashEntries");

            // 收集所有变量
            var variables = new List<ParameterExpression>();

            // 创建对象实例
            // T result = default(T);
            var resultVar = Expression.Variable(typeof(T), "result");
            expressions.Add(Expression.Assign(resultVar, Expression.New(typeof(T))));
            variables.Add(resultVar);

            foreach (var property in properties)
            {
                var fieldName = GetFieldName(property);
                // string [fieldName] = fileName;
                var fieldNameConst = Expression.Constant(fieldName);

                // 使用 Array.Find 方法查找匹配的 HashEntry
                // var entry = Array.Find(hashEntries, (x) => x.Name.ToString().Equals([fieldName], StringComparer.OrdinalIgnoreCase);
                var findMethod = typeof(Array).GetMethods().First(m => m.Name == "Find" && m.GetParameters().Length == 2).MakeGenericMethod(typeof(HashEntry));

                var predicateParam = Expression.Parameter(typeof(HashEntry), "x");
                var predicateBody = Expression.Call(
                        Expression.Call(Expression.Property(predicateParam, "Name"), "ToString", null),
                        typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(StringComparison) }),
                        fieldNameConst,
                        Expression.Constant(StringComparison.OrdinalIgnoreCase)
                );
                var predicateLambda = Expression.Lambda(
                    typeof(Predicate<HashEntry>),
                    predicateBody,
                    predicateParam
                    );

                var entryVar = Expression.Variable(typeof(HashEntry), $"entry_{property.Name}");
                expressions.Add(Expression.Assign(
                    entryVar,
                    Expression.Call(findMethod, hashEntriesParam, predicateLambda)
                ));
                variables.Add(entryVar);

                // 检查是否找到有效的 HashEntry
                var defaultHashEntry = Expression.Default(typeof(HashEntry));
                var hasValue = Expression.NotEqual(entryVar, defaultHashEntry);

                // 设置属性值
                var setPropertyBlock = Expression.IfThen(
                    hasValue,
                    CreateSetPropertyExpression(
                        resultVar,
                        property,
                        Expression.Property(entryVar, "Value")
                    )
                );

                expressions.Add(setPropertyBlock);
            }

            expressions.Add(resultVar);

            var block = Expression.Block(
                variables,
                expressions
            );

            var lambda = Expression.Lambda<Func<HashEntry[], T>>(block, hashEntriesParam);
            var compiled = lambda.Compile();
            return entries => compiled(entries);
        }

        /// <summary>
        /// 创建设置属性值的表达式
        /// </summary>
        private static Expression CreateSetPropertyExpression(Expression target, PropertyInfo property, Expression valueExpr)
        {
            var propertyExpr = Expression.Property(target, property);
            var convertedValueExpr = CreateValueConversionExpression(valueExpr, property.PropertyType);

            return Expression.IfThen(
                Expression.AndAlso(
                    Expression.NotEqual(valueExpr, Expression.Constant(RedisValue.Null)),
                    Expression.NotEqual(valueExpr, Expression.Constant(RedisValue.EmptyString))
                ),
                Expression.Assign(propertyExpr, convertedValueExpr)
            );
        }

        /// <summary>
        /// 创建值转换表达式
        /// </summary>
        private static Expression CreateValueConversionExpression(Expression valueExpr, Type targetType)
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            var isNullable = targetType != underlyingType;

            // 处理字符串类型
            if (underlyingType == typeof(string))
            {
                var toStringMethod = typeof(RedisValue).GetMethod("ToString", Type.EmptyTypes);
                return Expression.Call(valueExpr, toStringMethod);
            }

            if (underlyingType == typeof(string) ||
                   underlyingType == typeof(int) || underlyingType == typeof(int?) ||
                   underlyingType == typeof(long) || underlyingType == typeof(long?) ||
                   underlyingType == typeof(double) || underlyingType == typeof(double?) ||
                   underlyingType == typeof(float) || underlyingType == typeof(float?) ||
                   underlyingType == typeof(decimal) || underlyingType == typeof(decimal?) ||
                   underlyingType == typeof(uint) || underlyingType == typeof(uint?) ||
                   underlyingType == typeof(ulong) || underlyingType == typeof(ulong?) ||
                   underlyingType == typeof(bool) || underlyingType == typeof(bool?) ||
                   underlyingType == typeof(byte[]) ||
                   underlyingType == typeof(Memory<byte>) ||
                   underlyingType == typeof(ReadOnlyMemory<byte>)) // 检查是否支持隐式转换
            {
                return Expression.Convert(valueExpr, targetType);
            }

            if (underlyingType == typeof(short) || underlyingType == typeof(short?))
            {
                var parseMethod = typeof(System.Convert).GetMethod("ToInt16", new[] { typeof(string) });
                var toStringCall1 = Expression.Call(valueExpr, "ToString", null);
                var parsed = Expression.Call(null, parseMethod, toStringCall1);
                if (isNullable)
                {
                    return Expression.Convert(parsed, targetType);
                }
                else
                {
                    return parsed;
                }
            }

            // 处理 DateTime
            if (underlyingType == typeof(DateTime))
            {
                var parseMethod = typeof(DateTime).GetMethod("Parse", new[] { typeof(string) });
                var toStringCall1 = Expression.Call(valueExpr, "ToString", null);
                var parsed = Expression.Call(null, parseMethod, toStringCall1);
                if (isNullable)
                {
                    return Expression.Convert(parsed, targetType);
                }
                else
                {
                    return parsed;
                }
            }

            // 处理复杂类型（JSON 反序列化）
            var deserializeMethod = typeof(Newtonsoft.Json.JsonConvert).GetMethod("DeserializeObject",
                new[] { typeof(string), typeof(Type) });
            var toStringCall = Expression.Call(valueExpr, "ToString", null);

            var deserialized = Expression.Call(
                null,
                deserializeMethod,
                toStringCall,
                Expression.Constant(underlyingType)
            );

            return Expression.Convert(deserialized, targetType);
        }

        #endregion

        #region ConvertToHashEntries

        private static readonly ConcurrentDictionary<Type, Func<object, HashEntry[]>> _converters = new ConcurrentDictionary<Type, Func<object, HashEntry[]>>();

        /// <summary>
        /// 将对象转换为 HashEntry 数组
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static HashEntry[] ToHashEntries(object obj)
        {
            if (obj == null)
                return Array.Empty<HashEntry>();

            var type = obj.GetType();
            var converter = _converters.GetOrAdd(type, CreateConverter);
            return converter(obj);
        }

        private static Expression BuildValueExpression(PropertyInfo prop, MemberExpression propValue)
        {

            if (Nullable.GetUnderlyingType(prop.PropertyType) != null)
            {
                return Expression.Condition(
                    Expression.Equal(propValue, Expression.Constant(null, prop.PropertyType)),
                    Expression.Constant(RedisValue.Null, typeof(RedisValue)),
                    Expression.Convert(propValue, typeof(RedisValue))
                );
            }
            else if (prop.PropertyType == typeof(string) ||
                   prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?) ||
                   prop.PropertyType == typeof(long) || prop.PropertyType == typeof(long?) ||
                   prop.PropertyType == typeof(double) || prop.PropertyType == typeof(double?) ||
                   prop.PropertyType == typeof(float) || prop.PropertyType == typeof(float?) ||
                   prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?) ||
                   prop.PropertyType == typeof(uint) || prop.PropertyType == typeof(uint?) ||
                   prop.PropertyType == typeof(ulong) || prop.PropertyType == typeof(ulong?) ||
                   prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?) ||
                   prop.PropertyType == typeof(byte[]) ||
                   prop.PropertyType == typeof(Memory<byte>) ||
                   prop.PropertyType == typeof(ReadOnlyMemory<byte>)) // 检查是否支持隐式转换
            {
                Expression valueExpr = Expression.Convert(propValue, typeof(RedisValue));

                if (!prop.PropertyType.IsValueType)
                {
                    valueExpr = Expression.Condition(
                        Expression.Equal(propValue, Expression.Constant(null, prop.PropertyType)),
                        Expression.Constant(RedisValue.Null, typeof(RedisValue)),
                        valueExpr
                    );
                }
                return valueExpr;
            }
            else if (prop.PropertyType == typeof(short))
            {
                return Expression.Convert(Expression.Convert(propValue, typeof(int)), typeof(RedisValue));
            }
            else if (prop.PropertyType == typeof(DateTime))
            {
                return Expression.Convert(Expression.Call(propValue, "ToString", null, null), typeof(RedisValue));
            }
            else if (prop.PropertyType.IsPrimitive)
            {
                return Expression.Convert(Expression.Call(propValue, "ToString", null, null), typeof(RedisValue));
            }
            else
            {
                // 不支持隐式转换的类型，需要序列化
                var serializeMethod = typeof(Newtonsoft.Json.JsonConvert).GetMethod("SerializeObject",
                    new[] { typeof(object) });

                return Expression.Condition(
                    Expression.Equal(propValue, Expression.Constant(null, prop.PropertyType)),
                    Expression.Constant(RedisValue.Null, typeof(RedisValue)),
                    Expression.Convert(
                        Expression.Call(null, serializeMethod,
                            Expression.Convert(propValue, typeof(object))),
                        typeof(RedisValue)
                    )
                );
            }
        }

        private static Func<object, HashEntry[]> CreateConverter(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead &&
                           p.GetIndexParameters().Length == 0 &&
                           !Attribute.IsDefined(p, typeof(RedisIgnoreAttribute)))
                .ToArray();

            var objParam = Expression.Parameter(typeof(object), "obj");
            var typedObj = Expression.Convert(objParam, type);

            var entries = new List<Expression>();
            var entryListVar = Expression.Variable(typeof(List<HashEntry>), "entries");

            entries.Add(Expression.Assign(entryListVar, Expression.New(typeof(List<HashEntry>))));

            foreach (var prop in properties)
            {
                // 获取字段名称（支持 RedisFieldName 特性）
                var fieldName = GetFieldName(prop);

                var propValue = Expression.Property(typedObj, prop);
                var valueExpression = BuildValueExpression(prop, propValue);

                var addEntry = Expression.Call(
                    entryListVar,
                    typeof(List<HashEntry>).GetMethod("Add"),
                    Expression.New(
                        typeof(HashEntry).GetConstructor(new[] { typeof(RedisValue), typeof(RedisValue) }),
                        Expression.Constant((RedisValue)fieldName),
                        valueExpression
                    )
                );

                entries.Add(addEntry);
            }

            entries.Add(Expression.Call(entryListVar, typeof(List<HashEntry>).GetMethod("ToArray")));

            var block = Expression.Block(new[] { entryListVar }, entries);
            return Expression.Lambda<Func<object, HashEntry[]>>(block, objParam).Compile();
        }

        private static string GetFieldName(PropertyInfo prop)
        {
            var fieldNameAttr = prop.GetCustomAttribute<RedisFieldNameAttribute>();
            return fieldNameAttr?.Name ?? prop.Name;
        }

        #endregion
    }
}
