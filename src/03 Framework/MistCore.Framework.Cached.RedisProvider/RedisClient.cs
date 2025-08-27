using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MistCore.Framework.Cached.RedisProvider
{
    /// <summary>
    /// RedisCache缓存操作类
    /// </summary>
    public class RedisClient
    {
        private IDatabase database;

        public RedisClient(IDatabase database)
        {
            this.database = database;
        }

        #region 键命令

        /// <summary>
        /// DEL key
        /// 若键存在的情况下，该命令用于删除键。
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public long DEL(params string[] keys)
        {
            if (keys.Length > 1)
            {
                return this.database.KeyDelete(keys.Select(c => (RedisKey)c).ToArray());
            }
            else if (keys.Length == 1)
            {
                return this.database.KeyDelete(keys[0]) ? 1 : 0;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// EXISTS key
        /// 用于检查键是否存在，若存在则返回 1，否则返回 0。
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public long EXISTS(params string[] keys)
        {
            if (keys.Length > 1)
            {
                return this.database.KeyExists(keys.Select(c => (RedisKey)c).ToArray());
            }
            else if (keys.Length == 1)
            {
                return this.database.KeyExists(keys[0]) ? 1 : 0;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// EXPIRE key
        /// 设置 key 的过期时间，以秒为单位。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public bool EXPIRE(string key, int seconds)
        {
            return this.database.KeyExpire(key, TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// PEXPIRE key
        /// 设置 key 的过期，以毫秒为单位。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public bool PEXPIRE(string key, int milliseconds)
        {
            return this.database.KeyExpire(key, TimeSpan.FromMilliseconds(milliseconds));
        }

        /// <summary>
        /// EXPIREAT key
        /// 该命令与 EXPIRE 相似，用于为 key 设置过期时间，不同在于，它的时间参数值采用的是时间戳格式。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public bool EXPIREAT(string key, DateTime dateTime)
        {
            return this.database.KeyExpire(key, dateTime);
        }

        /// <summary>
        /// PEXPIREAT key
        /// 与 PEXPIRE 相似，用于为 key 设置过期时间，采用以毫秒为单位的时间戳格式。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="millisecondsTimestamp"></param>
        /// <returns></returns>
        public bool PEXPIREAT(string key, long millisecondsTimestamp)
        {
            return this.database.KeyExpire(key, new DateTime(1970, 1, 1).AddMilliseconds(millisecondsTimestamp));
        }

        /// <summary>
        /// PERSIST key
        /// 该命令用于删除 key 的过期时间，然后 key 将一直存在，不会过期。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool PERSIST(string key)
        {
            return this.database.KeyPersist(key);
        }

        /// <summary>
        /// TTL key
        /// 用于检查 key 还剩多长时间过期，以秒为单位。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public double? TTL(string key)
        {
            return this.database.KeyTimeToLive(key)?.TotalSeconds;
        }

        #endregion

        #region 事务

        public void MULTI(Action<ITransaction> func)
        {
            var transaction = this.database.CreateTransaction(); //multi

            func(transaction);

            transaction.Execute();
        }

        #endregion

        #region STRIGN

        public bool SET(string key, object value, TimeSpan? timeSpan = null)
        {
            var json = JsonConvert.SerializeObject(value);
            if (timeSpan == null)
            {
                return this.database.StringSet(key, json);
            }
            else
            {
                return this.database.StringSet(key, json, timeSpan);
            }
        }

        public T GET<T>(string key)
        {
            var value = this.database.StringGet(key);
            if (!value.HasValue)
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(Convert.ToString(value));
        }

        public T GETRANGE<T>(string key, long start, long end) where T : class
        {
            var value = this.database.StringGetRange(key, start, end);
            if (!value.HasValue)
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(value);
        }

        public T GETSET<T>(string key, RedisValue value) where T : class
        {
            var value1 = this.database.StringGetSet(key, value);
            if (!value1.HasValue)
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(value1);
        }

        public IEnumerable<T> MGET<T>(params string[] keys) where T : class
        {
            foreach (var item in this.database.StringGet(keys.Select(c => (RedisKey)c).ToArray()))
            {
                yield return item as T;
            }
        }

        /// <summary>
        /// SETEX key seconds value
        /// 将值 value 存储到 key中 ，并将 key 的过期时间设为 seconds(以秒为单位)。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="seconds"></param>
        public bool SETEX(string key, RedisValue value, int seconds)
        {
            return this.database.StringSet(key, value, TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// MSET key value[key value...]
        /// 该命令允许同时设置多个键值对。
        /// </summary>
        /// <param name="values"></param>
        /// <param name="when"></param>
        /// <returns></returns>
        public bool MSET(KeyValuePair<string, RedisValue>[] values, When when = When.Always)
        {
            return this.database.StringSet(values.Select(c => new KeyValuePair<RedisKey, RedisValue>(c.Key, c.Value)).ToArray(), when);
        }
        #endregion

        #region SET
        /// <summary>
        /// SADD key member [member ...]
        /// 向集合中添加一个或者多个元素，并且自动去重。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public long SADD(string key, params RedisValue[] members)
        {
            if (members.Length > 1)
            {
                return this.database.SetAdd(key, members.Select(c => c).ToArray());
            }
            else if (members.Length == 1)
            {
                return this.database.SetAdd(key, members[0]) ? 1 : 0;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// SCARD key
        /// 返回集合中元素的个数。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long SCARD(string key)
        {
            return this.database.SetLength(key);
        }

        /// <summary>
        /// SISMEMBER key member
        /// 查看指定元素是否存在于集合中。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public bool SISMEMBER(string key, string member)
        {
            return this.database.SetContains(key, member);
        }

        /// <summary>
        /// SMEMBERS key
        /// 查看集合中所有元素。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> SMEMBERS<T>(string key) where T : class
        {
            var result = this.database.SetMembers(key);
            if (typeof(T) == typeof(string))
            {
                return result.ToStringArray().OfType<T>().ToList();
            }
            return result.Select(c => c as T).ToList();
        }

        /// <summary>
        /// SREM key member1 [member2]
        /// 删除一个或者多个元素，若元素不存在则自动忽略。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        public long SREM(string key, params RedisValue[] members)
        {
            if (members.Length > 1)
            {
                return this.database.SetRemove(key, members);
            }
            else if (members.Length == 1)
            {
                return this.database.SetRemove(key, members[0]) ? 1 : 0;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// SSCAN key cursor [match pattern] [count count]
        /// 该命令用来迭代的集合中的元素。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="cursor"></param>
        /// <param name="pattern"></param>
        /// <param name="pageOffset"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IEnumerable<T> SSCAN<T>(string key, int cursor = 0, RedisValue pattern = default, int pageSize = 10, int pageOffset = 0) where T : class
        {
            foreach (var item in this.database.SetScan(key, pattern, pageSize, cursor, pageOffset))
            {
                yield return item as T;
            }
        }

        /// <summary>
        /// 集合运算
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public List<T> SetCombine<T>(SetOperation operation, params string[] keys) where T : class
        {
            return this.database.SetCombine(operation, keys.Select(c => (RedisKey)c).ToArray()).Select(c => c as T).ToList();
        }

        /// <summary>
        /// 集合运算，结果存到 destination
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="destination">结果集key</param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public long SetCombineAndStore(SetOperation operation, string destination, string first, string second)
        {
            return this.database.SetCombineAndStore(operation, destination, first, second);
        }
        #endregion

    }
}