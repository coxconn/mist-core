using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MistCore.Framework.Cached.RedisProvider
{
    /// <summary>
    /// RedisCache缓存操作类
    /// </summary>
    public partial class RedisClient
    {
        private IDatabase database;

        public RedisClient(IDatabase database)
        {
            this.database = database ?? throw new ArgumentNullException(nameof(database));
        }

        #region Key

        /// <summary>
        /// DEL key
        /// 删除指定的 key 。不存在的 key 会被忽略
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>命令执行后的返回值表示被删除 key 的数量</returns>
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
        /// DUMP key
        /// 序列化给定 key ，并返回被序列化的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns>key 存在时返回列化之后的值，否则，返回 nill</returns>
        public byte[] DUMP(string key)
        {
            return this.database.KeyDump(key);
        }

        /// <summary>
        /// EXISTS key
        /// 检查给定 key 是否存在
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>命令的返回值代表 key 存在的数量，如果被检查的 key 都不存则返回 0</returns>
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
        /// 为给定 key 设置过期时间，以秒为单位
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <returns>设置成功时返回 1，当 key 不存时则返回 0</returns>
        public bool EXPIRE(string key, int seconds)
        {
            return this.database.KeyExpire(key, TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// EXPIREAT key
        /// EXPIREAT 命令和 EXPIRE 命令类似，都用于为 key 设置过期时间。不同在于 EXPIREAT 命令接受的时间参数是 UNIX 时间戳(unix timestamp)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dateTime"></param>
        /// <returns>设置成功时返回 1，当 key 不存时则返回 0</returns>
        public bool EXPIREAT(string key, DateTime dateTime)
        {
            return this.database.KeyExpire(key, dateTime);
        }

        /// <summary>
        /// PEXPIRE key
        /// 与 EXPIRE 命令类似，用于为 key 设置过期时间，采用以毫秒为单位的时间格式。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="milliseconds"></param>
        /// <returns>设置成功时返回 1，当 key 不存时则返回 0</returns>
        public bool PEXPIRE(string key, int milliseconds)
        {
            return this.database.KeyExpire(key, TimeSpan.FromMilliseconds(milliseconds));
        }

        /// <summary>
        /// PEXPIREAT key
        /// 与 EXPIREAT 命令类似，都用于为 key 设置过期时间。不同在于 PEXPIREAT 命令接受的时间参数是以毫秒为单位的 UNIX 时间戳(unix timestamp)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="millisecondsTimestamp"></param>
        /// <returns></returns>
        public bool PEXPIREAT(string key, long millisecondsTimestamp)
        {
            return this.database.KeyExpire(key, new DateTime(1970, 1, 1).AddMilliseconds(millisecondsTimestamp));
        }

        ///// <summary>
        ///// KEYS pattern
        ///// </summary>
        ///// <param name="pattern"></param>
        ///// <returns></returns>
        //public List<string> KEYS(string pattern)
        //{
        //    var server = this.database.Multiplexer.GetServer(this.database.Multiplexer.GetEndPoints().First());
        //    return server.Keys(pattern: pattern).Select(c => c.ToString()).ToList();
        //}

        /// <summary>
        /// MOVE key db
        /// 将当前数据库(默认为 db 0)的 key 移动到给定的数据库 db 当中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="db"></param>
        /// <returns>移动成功返回 1，如果移动失败则返回 0</returns>
        public bool MOVE(string key, int db)
        {
            return this.database.KeyMove(key, db);
        }

        /// <summary>
        /// PERSIST key
        /// 移除 key 的过期时间，key 将持久保持
        /// </summary>
        /// <param name="key"></param>
        /// <returns>过期时间移除成功时，则返回 1 。 如果 key 不存在或者 key 没有设置过期时间，则返回 0</returns>
        public bool PERSIST(string key)
        {
            return this.database.KeyPersist(key);
        }

        /// <summary>
        /// RANDOMKEY
        /// 从当前数据库中随机返回一个 key
        /// </summary>
        /// <returns>当数据库不为空时，返回一个 key；当数据库为空时返回 nil</returns>
        public string RANDOMKEY()
        {
            return this.database.KeyRandom();
        }

        /// <summary>
        /// RENAME key newkey
        /// 将 key 改名为 newkey 。当 key 和 newkey 相同或者 key 不存在时，返回一个错误。当 newkey 已经存在时， RENAME 命令将覆盖旧值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newKey"></param>
        /// <returns>修改成功是返回  OK，如果修改失败则返回一个错误，错误一般有两种情况，一是被修改的 key 不存在，二是 key 与 newkey 名字相同</returns>
        public bool RENAME(string key, string newKey, When when = When.Always)
        {
            return this.database.KeyRename(key, newKey, when);
        }

        ///// <summary>
        ///// SCAN cursor [MATCH pattern] [COUNT count]
        ///// </summary>
        ///// <param name="key"></param>
        //public IEnumerable<string> SCAN(string pattern, int pageSize = 250)
        //{
        //    var server = this.database.Multiplexer.GetServer(this.database.Multiplexer.GetEndPoints().First());
        //    var enumerator = server.Keys(pattern: pattern, pageSize: pageSize).GetEnumerator();
        //    while (enumerator.MoveNext())
        //    {
        //        yield return enumerator.Current;
        //    }
        //}

        /// <summary>
        /// TTL key
        /// 返回 key 的剩余过期时间，以秒为单位
        /// </summary>
        /// <param name="key"></param>
        /// <returns>返回 key 所剩余生存时间。若 key 不存在时，返回 -2；若 key 存在但没有设置剩余生存时间时，返回 -1</returns>
        public double? TTL(string key)
        {
            return this.database.KeyTimeToLive(key)?.TotalSeconds;
        }

        /// <summary>
        /// TYPE key
        /// 返回 key 所储存的值的类型
        /// </summary>
        /// <param name="key"></param>
        /// <returns>返回 key 的数据类型，比如 string、list、set、hash、zset 等，若果返回 none，则表明不存在该 key</returns>
        public JsonTokenType TYPE(string key)
        {
            var type = this.database.KeyType(key);
            return type switch
            {
                RedisType.String => JsonTokenType.String,
                RedisType.List => JsonTokenType.StartArray,
                RedisType.Set => JsonTokenType.StartArray,
                RedisType.SortedSet => JsonTokenType.StartArray,
                RedisType.Hash => JsonTokenType.StartObject,
                _ => JsonTokenType.None,
            };
        }

        #endregion

        #region Exec

        /// <summary>
        /// ExecTransaction
        /// 执行事务
        /// </summary>
        /// <param name="func"></param>
        public void ExecTransaction(Action<ITransaction> func)
        {
            var transaction = this.database.CreateTransaction(); //multi

            func(transaction);

            transaction.Execute();
        }

        /// <summary>
        /// Exec
        /// 执行命令
        /// </summary>
        /// <param name="func"></param>
        public void Exec(Action<IDatabase> func)
        {
            func(this.database);
        }
        #endregion

        #region Hash

        /// <summary>
        /// HDEL key field [field ...]
        /// 删除一个或多个哈希表字段
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fields"></param>
        /// <returns>被成功移除的字段的数量，不包括被忽略的字段</returns>
        public long HDEL(string key, params string[] fields)
        {
            if (fields.Length > 1)
            {
                return this.database.HashDelete(key, fields.Select(c => (RedisValue)c).ToArray());
            }
            else if (fields.Length == 1)
            {
                return this.database.HashDelete(key, fields[0]) ? 1 : 0;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// HEXISTS key field
        /// 查看哈希表 key 中，指定的字段是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <returns>果哈希表含有给字段，则返回 1；如果哈希表不含有给定字段，或者是 key 不存在，那么返回 0</returns>
        public long HEXISTS(string key, string field)
        {
            return this.database.HashExists(key, field) ? 1 : 0;
        }

        /// <summary>
        /// HGET key field
        /// 返回哈希表 key 中给定字段 field 的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <returns>给定 field 的值。当给定 filed 不存在或给定 key 不存在时，则返回 nil</returns>
        public T HGET<T>(string key, string field)
        {
            var result = this.database.HashGet(key, field);
            return result.ToObject<T>();
        }

        /// <summary>
        /// HGETALL key
        /// 返回哈希表 key 中，所有的字段和值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns>以列表形式返回哈希表的字段和字段值。若 key 不存在，则返回空列表</returns>
        public T HGETALL<T>(string key) where T : class, new()
        {
            var result = this.database.HashGetAll(key);
            return result.ToObject<T>();
        }

        /// <summary>
        /// HINCRBY key field increment
        /// 为哈希表 key 中的指定字段的整数值加上增量 increment
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns>执行 HINCRBY 命令之后，哈希表 key 中 field 的值</returns>
        public long HINCRBY(string key, string field, long value)
        {
            return this.database.HashIncrement(key, field, value);
        }

        /// <summary>
        /// HKEYS key
        /// 返回哈希表 key 中的所有字段
        /// </summary>
        /// <param name="key"></param>
        /// <returns>一个包含哈希表中所有字段的列表。当 key 不存在时，返回一个空列表</returns>
        public List<string> HKEYS(string key)
        {
            return this.database.HashKeys(key).Select(c => c.ToString()).ToList();
        }

        /// <summary>
        /// HLEN key
        /// 返回哈希表 key 中字段的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns>哈希表中字段的数量。若 key 不存在时，则返回 0</returns>
        public long HLEN(string key)
        {
            return this.database.HashLength(key);
        }

        /// <summary>
        /// HSET key field value
        /// 如果 field 是哈希表中的一个新字段，并且值设置成功，则返回 1；如果哈希表中 field 已经存在，并且旧值已被新值覆盖，则返回 0
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns>如果 field 是哈希表中的一个新字段，并且值设置成功，则返回 1；如果哈希表中 field 已经存在，并且旧值已被新值覆盖，则返回 0</returns>
        public long HSET<T>(string key, string field, T value)
        {
            return this.database.HashSet(key, field, RedisExtensions.ToRedisValue(value)) ? 1 : 0;
        }

        /// <summary>
        /// HSET key field value, field1 value1...
        /// 同时将多个 field-value (字段-值)对设置到哈希表 key 中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void HSET<T>(string key, T value) where T: class, new()
        {
            this.database.HashSet(key, RedisExtensions.ToHashEntries(value));
        }

        /// <summary>
        /// HVALS key
        /// 返回哈希表 key 中，所有字段的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns>一个包含哈希表中所有的字段值。当 key 不存在时，返回一个空列表</returns>
        public RedisValue[] HVALS(string key)
        {
            return this.database.HashValues(key);
        }

        #endregion

        #region String

        /// <summary>
        /// APPEND key value
        /// 如果 key 已经存在并且是一个字符串，APPEND 命令将 value 追加到 key 原来的值的末尾。如果 key 不存在，APPEND 就简单地将 key 的值设为 value，就像执行 SET key value 一样
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>追加 value 之后， key 存储的字符串长度</returns>
        public long APPEND(string key, string value)
        {
            return this.database.StringAppend(key, value);
        }

        /// <summary>
        /// BITCOUNT key [start end]
        /// 计算给定字符串中，被设置为 1 的比特位的数量
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns>被设置为 1 的位的数量</returns>
        public long BITCOUNT(string key, long start = 0, long end = -1)
        {
            return this.database.StringBitCount(key, start, end);
        }

        /// <summary>
        /// DECR key
        /// 将 key 中储存的数字值减一
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>操作后 key 的值</returns>
        public long DECR(string key)
        {
            return this.database.StringDecrement(key);
        }

        /// <summary>
        /// DECRBY key decrement
        /// 将 key 所储存的值减去减量 decrement
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>操作后 key 的值</returns>
        public long DECRBY(string key, long value = -1)
        {
            return this.database.StringDecrement(key, value);
        }

        /// <summary>
        /// DECRBYFLOAT key decrement
        /// 将 key 所储存的值减去减量 decrement
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>操作后 key 的值</returns>
        public double DECRBYFLOAT(string key, double value = -1)
        {
            return this.database.StringDecrement(key, value);
        }

        /// <summary>
        /// INCR key
        /// 将 key 中储存的数字值增一
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>操作后 key 的值</returns>
        public long INCR(string key)
        {
            return this.database.StringIncrement(key);
        }

        /// <summary>
        /// INCRBY key increment
        /// 将 key 所储存的值加上增量 increment
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>操作后 key 的值</returns>
        public long INCRBY(string key, long value = 1)
        {
            return this.database.StringIncrement(key, value);
        }

        /// <summary>
        /// INCRBYFLOAT key increment
        /// 将 key 所储存的值加上增量 increment
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>操作后 key 的值</returns>
        public double INCRBYFLOAT(string key, double value = 1)
        {
            return this.database.StringIncrement(key, value);
        }

        /// <summary>
        /// GET key
        /// 返回 key 所关联的字符串值
        /// </summary>
        /// <param name="key"></param>
        /// <returns>当 key 不存在时，返回 nil ，否则，返回 key 的值。如果 key 不是字符串类型，那么返回一个错误</returns>
        public string GET(string key)
        {
            return this.database.StringGet(key);
        }

        /// <summary>
        /// GETBIT key offset
        /// 对 key 所储存的字符串值，获取指定偏移量上的位(bit)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="offset"></param>
        /// <returns>字符串值指定偏移量上的位(bit)</returns>
        public bool GETBIT(string key, long offset)
        {
            return this.database.StringGetBit(key, offset);
        }

        /// <summary>
        /// GETRANGE key start end
        /// 返回 key 中字符串值的子字符串，字符串的截取范围由偏移量 start 和 end 决定(包括 start 和 end 在内)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns>截取后得到的子字符串</returns>
        public string GETRANGE(string key, long start = 0, long end = -1)
        {
            return this.database.StringGetRange(key, start, end);
        }

        /// <summary>
        /// GETSET key value
        /// 将给定 key 的值设为 value ，并返回 key 的旧值(old value)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>返回给定 key 的旧值，若 key 不存在则返回 nil；当 key 存在但不是字符类型是，返回一个错误</returns>
        public string GETSET(string key, string value)
        {
            return this.database.StringGetSet(key, value);
        }

        /// <summary>
        /// MGET key [key ...]
        /// 返回所有(一个或多个)给定 key 的值
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>返回所有 key 存储的 value 值</returns>
        public IEnumerable<string> MGET(params string[] keys)
        {
            var values = this.database.StringGet(keys.Select(c => (RedisKey)c).ToArray());
            foreach(var value in values)
            {
                yield return value.ToObject<string>();
            }
        }

        /// <summary>
        /// MSET key value [key value ...]
        /// 同时设置一个或多个 key-value 对。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="kvs"></param>
        /// <param name="when"></param>
        /// <returns>返回OK</returns>
        public bool MSET<T>(KeyValuePair<string, T>[] kvs, When when = When.Always)
        {
            return this.database.StringSet(kvs.Select(c => new KeyValuePair<RedisKey, RedisValue>(c.Key, RedisExtensions.ToRedisValue(c.Value))).ToArray(), when);
        }

        /// <summary>
        /// MSETNX key value [key value ...]
        /// 只有在所有给定 key 都不存在的情况下，才设置 key-value 对。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="kvs"></param>
        /// <returns>当所有 key 都成功设置时，返回 1，如果其中至少一个 key 已经存在，那么将设置失败，此时会返回 0</returns>
        public bool MSETNX<T>(KeyValuePair<string, string>[] kvs)
        {
            return this.database.StringSet(kvs.Select(c => new KeyValuePair<RedisKey, RedisValue>(c.Key, RedisExtensions.ToRedisValue(c.Value))).ToArray(), When.NotExists);
        }

        /// <summary>
        /// SET key value [EX seconds] [PX milliseconds] [NX|XX]
        /// 将字符串值 value 关联到 key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <param name="when"></param>
        /// <returns>返回OK</returns>
        public bool SET(string key, string value, TimeSpan? expiry = null, When when = When.Always)
        {
            return this.database.StringSet(key, value, expiry, when: When.Always);
        }

        /// <summary>
        /// SETNX key value [EX seconds] [PX milliseconds]
        /// 只有在 key 不存在时，设置 key 的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns>设置成功，返回 1；设置失败，返回 0</returns>
        public bool SETNX(string key, string value, TimeSpan? expiry = null)
        {
            return this.database.StringSet(key, value, expiry, when: When.NotExists);
        }

        /// <summary>
        /// SETEX key seconds value
        /// 将值 value 存储到 key中 ，并将 key 的过期时间设为 seconds(以秒为单位)。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="seconds"></param>
        /// <param name="when"></param>
        /// <returns>设置成功时返回 OK，若 second 参数不符合要求，则会返回一个错误，比如设置成了负数或者浮点数</returns>
        public bool SETEX(string key, string value, int seconds, When when = When.Always)
        {
            return this.database.StringSet(key, value, TimeSpan.FromSeconds(seconds), when: when);
        }

        /// <summary>
        /// SETBIT key offset value
        /// 对 key 所储存的字符串值，设置或清除指定偏移量上的位(bit)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="offset"></param>
        /// <param name="value"></param>
        /// <returns>返回值指定了偏移量原来储存的位</returns>
        public bool SETBIT(string key, long offset, bool value)
        {
            return this.database.StringSetBit(key, offset, value);
        }

        /// <summary>
        /// STRLEN key
        /// 返回 key 所储存的字符串值的长度
        /// </summary>
        /// <param name="key"></param>
        /// <returns>返回字符串值的长度。当 key 不存在时，返回 0</returns>
        public long STRLEN(string key)
        {
            return this.database.StringLength(key);
        }

        /// <summary>
        /// SETRANGE key offset value
        /// 用 value 参数覆写(overwrite)给定 key 所储存的字符串值，从偏移量 offset 开始
        /// </summary>
        /// <param name="key"></param>
        /// <param name="offset"></param>
        /// <param name="value"></param>
        /// <returns>被修改之后的字符串总长度</returns>
        public string SETRANGE(string key, long offset, string value)
        {
            return this.database.StringSetRange(key, offset, value).ToString();
        }

        #endregion

        #region List

        /// <summary>
        /// LPOP key count
        /// 弹出头部count 个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns>返回值是列表的头部的count 个元素。当 key 不存在时，则返回 nil</returns>
        public IEnumerable<T> LPOP<T>(string key, long count = 1)
        {
            var values = this.database.ListLeftPop(key, count);
            foreach(var value in values)
            {
                yield return value.ToObject<T>();
            }
        }

        /// <summary>
        /// LPOP [key...] count
        /// 按 key 的先后顺序依次检查各个列表，弹出第一个非空列表的头 count 个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <param name="count"></param>
        /// <returns>返回一个含有两个元素的列表，第一个元素是被弹出元素所属的 key，第二个元素是被弹出元素的值</returns>
        public (string, List<T>) LPOP<T>(string[] keys, long count = 1)
        {
            var result = this.database.ListLeftPop(keys.Select(c => (RedisKey)c).ToArray(), count);
            return (result.Key, result.Values.Select(c => c.ToObject<T>()).ToList());
        }

        /// <summary>
        /// RPOP key count
        /// 弹出尾部count 个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns>返回值是列表的尾部的count 个元素。当 key 不存在时，则返回 nil</returns>
        public IEnumerable<T> RPOP<T>(string key, long count = 1)
        {
            var values = this.database.ListRightPop(key, count);
            foreach (var value in values)
            {
                yield return value.ToObject<T>();
            }
        }

        /// <summary>
        /// RPOP [key...] count
        /// 按 key 的先后顺序依次检查各个列表，弹出第一个非空列表的头 count 个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <param name="count"></param>
        /// <returns>返回一个含有两个元素的列表，第一个元素是被弹出元素所属的 key，第二个元素是被弹出元素的值</returns>
        public (string, List<T>) RPOP<T>(string[] keys, long count = 1)
        {
            var result = this.database.ListRightPop(keys.Select(c => (RedisKey)c).ToArray(), count);
            return (result.Key, result.Values.Select(c => c.ToObject<T>()).ToList());
        }

        /// <summary>
        /// BRPOPLPUSH source destination
        /// 阻塞式弹出列表的尾元素(右边)，并将该元素添加到另一个列表的头部(左边)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="destination"></param>
        /// <returns>被弹出的元素</returns>
        public T BRPOPLPUSH<T>(string key, string destination)
        {
            var result = this.database.ListRightPopLeftPush(key, destination);
            return result.ToObject<T>();
        }

        /// <summary>
        /// LINDEX key index
        /// 返回列表 key 中，下标为 index 的元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <returns>列表中下标为 index 的元素。如果 index 参数值超出列表的区间范围(out of range)，则返回 nil</returns>
        public T LINDEX<T>(string key, long index)
        {
            var result = this.database.ListGetByIndex(key, index);
            return result.ToObject<T>();
        }

        /// <summary>
        /// LINSERT key BEFORE|AFTER pivot value
        /// 将值 value 插入到列表 key 当中，位于值 pivot 之前或之后
        /// </summary>
        /// <param name="key"></param>
        /// <param name="listInsert"></param>
        /// <param name="pivot"></param>
        /// <param name="value"></param>
        /// <returns>如果命令执行成功，返回插入操作完成之后，列表的长度；如果没有找到 pivot ，返回 -1 。如果 key 不存在或为空列表，返回 0 </returns>
        public long LINSERT(string key, ListInsert listInsert, RedisValue pivot, RedisValue value)
        {
            if (listInsert == ListInsert.Before)
                return this.database.ListInsertBefore(key, pivot, value);
            else
                return this.database.ListInsertAfter(key, pivot, value);
        }

        /// <summary>
        /// LLEN key
        /// 返回列表 key 的长度
        /// </summary>
        /// <param name="key"></param>
        /// <returns>列表 key 的长度</returns>
        public long LLEN(string key)
        {
            return this.database.ListLength(key);
        }

        /// <summary>
        /// LPUSH key value [value ...]
        /// 将一个或多个值插入到列表头部（从左侧开始操作），如果有多个 value 值，那么各个 value 值按从左到右的顺序依次插入
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <param name="when"></param>
        /// <returns>列表的长度</returns>
        public long LPUSH<T>(string key, T[] values, When when = When.Always)
        {
            if (values.Length > 1)
            {
                return this.database.ListLeftPush(key, values.Select(c => RedisExtensions.ToRedisValue(c)).ToArray(), when: when);
            }
            else if (values.Length == 1)
            {
                return this.database.ListLeftPush(key, RedisExtensions.ToRedisValue(values[0]), when: when);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// LPUSHX key value [value ...]
        /// 仅当键已经存在时才执行设置操作，将一个或多个值插入到列表头部（从左侧开始操作），如果有多个 value 值，那么各个 value 值按从左到右的顺序依次插入
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns>列表的长度</returns>
        public long LPUSHX<T>(string key, T[] values)
        {
            if (values.Length > 1)
            {
                return this.database.ListLeftPush(key, values.Select(c => RedisExtensions.ToRedisValue(c)).ToArray(), When.Exists);
            }
            else if (values.Length == 1)
            {
                return this.database.ListLeftPush(key, RedisExtensions.ToRedisValue(values[0]), When.Exists);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// RPUSH key value [value ...]
        /// 将一个或多个值插入到列表尾部（从右侧开始操作），如果有多个 value 值，那么各个 value 值按从右到左的顺序依次插入
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <param name="when"></param>
        /// <returns>列表的长度</returns>
        public long RPUSH<T>(string key, T[] values, When when = When.Always)
        {
            if (values.Length > 1)
            {
                return this.database.ListRightPush(key, values.Select(c => RedisExtensions.ToRedisValue(c)).ToArray(), when: when);
            }
            else if (values.Length == 1)
            {
                return this.database.ListRightPush(key, RedisExtensions.ToRedisValue(values[0]), when: when);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// RPUSHHX key value [value ...]
        /// 仅当键不存在时才执行设置操作，将一个或多个值插入到列表尾部（从右侧开始操作），如果有多个 value 值，那么各个 value 值按从右到左的顺序依次插入
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns>列表的长度</returns>
        public long RPUSHHX<T>(string key, params T[] values)
        {
            if (values.Length > 1)
            {
                return this.database.ListRightPush(key, values.Select(c => RedisExtensions.ToRedisValue(c)).ToArray(), When.Exists);
            }
            else if (values.Length == 1)
            {
                return this.database.ListRightPush(key, RedisExtensions.ToRedisValue(values[0]), When.Exists);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// LRANGE key start stop
        /// 返回列表 key 中指定区间内的元素，区间以偏移量 start 和 stop 指定。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns>一个列表，包含指定区间内的元素</returns>
        public IEnumerable<T> LRANGE<T>(string key, long start = 0, long stop = -1)
        {
            var values = this.database.ListRange(key, start, stop);
            foreach(var value in values)
            {
                yield return value.ToObject<T>();
            }
        }

        /// <summary>
        /// LRANGE key pageIndex pageSize
        /// 分页获取列表元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>一个列表，包含指定区间内的元素</returns>
        public IEnumerable<T> LRANGE<T>(string key, int pageIndex, int pageSize)
        {
            var start = (pageIndex - 1) * pageSize;
            var stop = start + pageSize - 1;
            var values = this.database.ListRange(key, start, stop);
            foreach (var value in values)
            {
                yield return value.ToObject<T>();
            }
        }   

        /// <summary>
        /// LREM key count value
        /// 根据参数 count 的值，移除列表中与参数 value 相等的元素。
        /// count 取值有以下几种情况：
        /// count > 0 : 从表头开始向表尾搜索，移除与 value 相等的元素，数量为 count 。
        /// count< 0 : 从表尾开始向表头搜索，移除与 value 相等的元素，数量为 count 的绝对值。
        /// count = 0 : 移除表中所有与 value 相等的值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <returns>被移除元素的数量。若 key 不存在时， 则 LREM 命令总是返回 0</returns>
        public long LREM<T>(string key, T value, long count = 0)
        {
            return this.database.ListRemove(key, RedisExtensions.ToRedisValue(value), count);
        }

        /// <summary>
        /// LSET key index value
        /// 将列表中下标为 index 的元素的值设置为 value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns>操作成功返回 ok ，否则返回错误信息</returns>
        public T LSET<T>(string key, long index, T value) where T : class
        {
            this.database.ListSetByIndex(key, index, RedisExtensions.ToRedisValue(value));
            var result = this.database.ListGetByIndex(key, index);
            return result.ToObject<T>();
        }

        /// <summary>
        /// LTRIM key start stop
        /// 对一个列表进行修剪(trim)，就是说，让列表只保留指定区间内的元素，不在指定区间之内的元素都将被删除。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        public void LTRIM(string key, long start, long stop)
        {
            this.database.ListTrim(key, start, stop);
        }

        #endregion

        #region SET

        /// <summary>
        /// SADD key member [member ...]
        /// 将一个或多个元素加入到集合中，已经存在于集合的元素将被忽略。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>被添加到集合中的新元素的数量，不包括被忽略的元素</returns>
        public long SADD<T>(string key, params T[] members)
        {
            if (members.Length > 1)
            {
                return this.database.SetAdd(key, members.Select(c => RedisExtensions.ToRedisValue(c)).ToArray());
            }
            else if (members.Length == 1)
            {
                return this.database.SetAdd(key, RedisExtensions.ToRedisValue(members[0])) ? 1 : 0;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// SCARD key
        /// 返回集合中元素的数量。
        /// </summary>
        /// <param name="key"></param>
        /// <returns>集合的基数。若 key 不存在时，则返回 0</returns>
        public long SCARD(string key)
        {
            return this.database.SetLength(key);
        }

        /// <summary>
        /// SISMEMBER key member
        /// 判断 member 元素是否集合 key 的成员。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <returns>是否存在</returns>
        public bool SISMEMBER<T>(string key, T member)
        {
            return this.database.SetContains(key, RedisExtensions.ToRedisValue(member));
        }

        /// <summary>
        /// SMEMBERS key
        /// 返回集合 key 中的所有成员。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns>集合中的所有成员</returns>
        public IEnumerable<T> SMEMBERS<T>(string key)
        {
            var values = this.database.SetMembers(key);
            foreach(var value in values)
            {
                yield return value.ToObject<T>();
            }
        }

        /// <summary>
        /// SMOVE source destination member
        /// 将 member 元素从 source 集合移动到 destination 集合。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="member"></param>
        /// <returns>如果 member 元素被成功移除，返回 1。如果 member 元素不是 source 集合的成员，那么将返回 0</returns>
        public long SMOVE<T>(string source, string destination, T member)
        {
            return this.database.SetMove(source, destination, RedisExtensions.ToRedisValue(member)) ? 1 : 0;
        }

        /// <summary>
        /// SPOP key [count]
        /// 移除并返回集合中的一个或多个随机元素。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns>被移除的随机元素。当 key 不存在或 key 是空集时，返回 nil</returns>
        public long SPOP<T>(string key, long count = 1)
        {
            var result = this.database.SetPop(key, count);
            return result.Select(c => c.ToObject<T>()).Count();
        }

        /// <summary>
        /// SRANDMEMBER key [count]
        /// 返回集合中的一个或多个随机元素。
        /// 如果 count 为正数，且小于集合基数，那么命令返回一个包含 count 个元素的数组，数组中的元素各不相同。
        /// 如果 count 大于等于集合基数，那么返回整个集合。
        /// 如果 count 为负数，那么返回数组中的元素可能会重复出现多次，而数组的长度为 count 的绝对值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns>只提供 key 参数时，返回一个元素；如果集合为空，返回 nil 。如果提供了 count 参数，那么返回一个数组，此时若集合为空，则返回空数组</returns>
        public IEnumerable<T> SRANDMEMBER<T>(string key, long count = 1)
        {
            var values = this.database.SetRandomMembers(key, count);
            foreach (var value in values)
            {
                yield return value.ToObject<T>();
            }
        }

        /// <summary>
        /// SREM key member1 [member2]
        /// 移除集合中的一个或多个成员元素，不存在的成员元素会被忽略。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="members"></param>
        /// <returns>被成功移除的元素的数量，不包括被忽略的元素</returns>
        public long SREM<T>(string key, params T[] members)
        {
            if (members.Length > 1)
            {
                return this.database.SetRemove(key, members.Select(c => RedisExtensions.ToRedisValue(c)).ToArray());
            }
            else if (members.Length == 1)
            {
                return this.database.SetRemove(key, RedisExtensions.ToRedisValue(members[0])) ? 1 : 0;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// SSCAN key cursor [match pattern] [count count]
        /// 迭代集合中的元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="cursor"></param>
        /// <param name="pattern"></param>
        /// <param name="pageOffset"></param>
        /// <param name="pageSize"></param>
        /// <returns>集合中的元素列表</returns>
        public IEnumerable<T> SSCAN<T>(string key, int cursor = 0, string pattern = default, int pageSize = 10, int pageOffset = 0)
        {
            var values = this.database.SetScan(key, pattern, pageSize, cursor, pageOffset);
            foreach (var value in values)
            {
                yield return value.ToObject<T>();
            }
        }

        /// <summary>
        /// SDIFF key [key ...]
        /// 所有给定集合的差集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns>集合的差集</returns>
        public IEnumerable<T> SDIFF<T>(params string[] keys)
        {
            var values = this.database.SetCombine(SetOperation.Difference, keys.Select(c => (RedisKey)c).ToArray());
            foreach(var value in values)
            {
                yield return value.ToObject<T>();
            }
        }

        /// <summary>
        /// SINTER key [key ...]
        /// 所有给定集合的交集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns>集合的交集</returns>
        public IEnumerable<T> SINTER<T>(params string[] keys)
        {
            var values = this.database.SetCombine(SetOperation.Intersect, keys.Select(c => (RedisKey)c).ToArray());
            foreach (var value in values)
            {
                yield return value.ToObject<T>();
            }
        }

        /// <summary>
        /// SUNION key [key ...]
        /// 所有给定集合的并集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns>集合的并集</returns>
        public IEnumerable<T> SUNION<T>(params string[] keys)
        {
            var values = this.database.SetCombine(SetOperation.Union, keys.Select(c => (RedisKey)c).ToArray());
            foreach (var value in values)
            {
                yield return value.ToObject<T>();
            }
        }

        /// <summary>
        /// SDIFFSTORE destination key [key ...]
        /// SDIFFSTORE 命令将给定集合的差集存储在指定的集合中
        /// </summary>
        /// <param name="destination">结果集key</param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>结果集中的元素数量</returns>
        public long SDIFFSTORE(string destination, string first, string second)
        {
            return this.database.SetCombineAndStore(SetOperation.Difference, destination, first, second);
        }

        /// <summary>
        /// SINTERSTORE destination key [key ...]
        /// SINTERSTORE 命令将给定集合的交集存储在指定的集合中
        /// </summary>
        /// <param name="destination">结果集key</param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>结果集中的元素数量</returns>
        public long SINTERSTORE(string destination, string first, string second)
        {
            return this.database.SetCombineAndStore(SetOperation.Intersect, destination, first, second);
        }

        /// <summary>
        /// SUNIONSTORE destination key [key ...]
        /// SUNIONSTORE 命令将给定集合的并集存储在指定的集合中
        /// </summary>
        /// <param name="destination">结果集key</param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>结果集中的元素数量</returns>
        public long SUNIONSTORE(string destination, string first, string second)
        {
            return this.database.SetCombineAndStore(SetOperation.Union, destination, first, second);
        }

        #endregion

        #region ZSET

        /// <summary>
        /// ZADD key [NX|XX] [CH] [INCR] score member
        /// 将一个 member 元素及其 score 值加入到有序集 key 当中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="score"></param>
        /// <param name="when"></param>
        /// <returns>被成功添加的新成员的数量，不包括那些被更新的，或者已经存在的成员</returns>
        public long ZADD<T>(string key, T value, double score, SortedSetWhen when = SortedSetWhen.Always)
        {
            return this.database.SortedSetAdd(key, RedisExtensions.ToRedisValue(value), score, when: when) ? 1 : 0;
        }

        /// <summary>
        /// ZADD key [NX|XX] [CH] [INCR] score member [score member ...]
        /// 将一个或多个 member 元素及其 score 值加入到有序集 key 当中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <param name="when"></param>
        /// <returns>被成功添加的新成员的数量，不包括那些被更新的，或者已经存在的成员</returns>
        public long ZADD<T>(string key, KeyValuePair<T, double>[] values, SortedSetWhen when = SortedSetWhen.Always)
        {
            if (values.Length > 1)
            {
                return this.database.SortedSetAdd(key, values.Select(c => new SortedSetEntry(RedisExtensions.ToRedisValue(c.Key), c.Value)).ToArray(), when: when);
            }
            else if (values.Length == 1)
            {
                return this.database.SortedSetAdd(key, RedisExtensions.ToRedisValue(values[0].Key), values[0].Value, when: when) ? 1 : 0;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// ZCARD key
        /// 返回有序集 key 的基数。
        /// </summary>
        /// <param name="key"></param>
        /// <returns>当 key 存在且是有序集类型时，返回有序集的基数。当 key 不存在时，返回 0 </returns>
        public long ZCARD(string key)
        {
            return this.database.SortedSetLength(key);
        }

        /// <summary>
        /// ZCOUNT key min max
        /// 返回有序集 key 中， score 值在 min 和 max 之间(默认包括 score 值等于 min 或 max )的成员的数量。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>score 值在 min 和 max 之间的成员的数量</returns>
        public long ZCOUNT(string key, double min, double max)
        {
            return this.database.SortedSetLength(key, min, max);
        }

        /// <summary>
        /// ZINCRBY key increment member
        /// 为有序集 key 的成员 member 的 score 值加上增量 increment
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <param name="value"></param>
        /// <returns>member 成员的新 score 值</returns>
        public double ZINCRBY<T>(string key, T member, double value)
        {
            return this.database.SortedSetIncrement(key, RedisExtensions.ToRedisValue(member), value);
        }

        /// <summary>
        /// ZRANGE key start stop [WITHSCORES]
        /// 返回有序集 key 中，指定区间内的成员。
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="weight1"></param>
        /// <param name="weight2"></param>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        public double ZINTERSTORE(string destination, string first, string second, double weight1 = 1, double weight2 = 1, Aggregate aggregate = Aggregate.Sum)
        {
            return this.database.SortedSetCombineAndStore(SetOperation.Intersect, destination, new RedisKey[] { first, second }, new double[] { weight1, weight2 }, aggregate);
        }

        /// <summary>
        /// ZRANGE key start stop [WITHSCORES]
        /// 返回有序集 key 中，指定区间内的成员。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="order"></param>
        /// <returns>指定区间内，带有 score 值(可选)的有序集成员的列表</returns>
        public IEnumerable<T> ZRANGE<T>(string key, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            var values = this.database.SortedSetRangeByRank(key, start, stop, order);
            foreach (var value in values)
            {
                yield return value.ToObject<T>();
            }
        }

        /// <summary>
        /// ZRANGEBYSCORE key min max [WITHSCORES] [LIMIT offset count]
        /// 返回有序集 key 中，指定 score 区间内的成员。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="exclude"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public IEnumerable<T> TRANGEBYLEX<T>(string key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None, long skip = 0, long take = -1)
        {
            var values = this.database.SortedSetRangeByValue(key, min, max, exclude, skip, take);
            foreach (var value in values)
            {
                yield return value.ToObject<T>();
            }
        }

        /// <summary>
        /// ZRANGE key start stop [WITHSCORES]
        /// 返回有序集 key 中，指定区间内的成员。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="order"></param>
        /// <returns>指定成员范围的元素列表</returns>
        public IEnumerable<(T, double)> ZRANGEWITHSCORES<T>(string key, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            var values = this.database.SortedSetRangeByRankWithScores(key, start, stop, order);
            foreach (var value in values)
            {
                yield return (value.Element.ToObject<T>(), value.Score);
            }
        }

        /// <summary>
        /// ZRANK key member
        /// 返回有序集 key 中成员 member 的排名。其中有序集成员按 score 值递增(从小到大)顺序排列。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <returns>如果 member 是有序集 key 的成员，返回 member 的排名；如果 member 不是有序集 key 的成员，返回 nil </returns>
        public long? ZRANK<T>(string key, T member)
        {
            var rank = this.database.SortedSetRank(key, RedisExtensions.ToRedisValue(member));
            return rank;
        }

        /// <summary>
        /// ZREM key member [member ...]
        /// 移除有序集 key 中的一个或多个成员，不存在的成员将被忽略。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="members"></param>
        /// <returns>被成功移除的成员的数量，不包括被忽略的成员</returns>
        public long ZREM<T>(string key, params T[] members)
        {
            if (members.Length > 1)
            {
                return this.database.SortedSetRemove(key, members.Select(c => RedisExtensions.ToRedisValue(c)).ToArray());
            }
            else if (members.Length == 1)
            {
                return this.database.SortedSetRemove(key, RedisExtensions.ToRedisValue(members[0])) ? 1 : 0;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// ZREMRANGEBYRANK key start stop
        /// 移除有序集 key 中，指定排名(rank)区间内的所有成员。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns>被移除成员的数量</returns>
        public long ZREMRANGEBYRANK(string key, long start, long stop)
        {
            return this.database.SortedSetRemoveRangeByRank(key, start, stop);
        }

        /// <summary>
        /// ZREMRANGEBYSCORE key min max
        /// 移除有序集 key 中，所有 score 值介于 min 和 max 之间(默认包括 score 值等于 min 或 max )的成员。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>被移除成员的数量</returns>
        public long ZREMRANGEBYSCORE(string key, double min, double max)
        {
            return this.database.SortedSetRemoveRangeByScore(key, min, max);
        }

        /// <summary>
        /// ZREVRANGE key start stop [WITHSCORES]
        /// 返回有序集 key 中，指定区间内的成员。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns>指定区间内，带有 score 值(可选)的有序集成员的列表</returns>
        public IEnumerable<T> ZREVRANGE<T>(string key, long start = 0, long stop = -1)
        {
            var values = this.database.SortedSetRangeByRank(key, start, stop, Order.Descending);
            foreach (var value in values)
            {
                yield return value.ToObject<T>();
            }
        }

        /// <summary>
        /// ZREVRANK key member
        /// 返回有序集 key 中成员 member 的排名。其中有序集成员按 score 值递减(从大到小)顺序排列。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <param name="order"></param>
        /// <returns>如果 member 是有序集 key 的成员，返回 member 的排名；如果 member 不是有序集 key 的成员，返回 nil </returns>
        public long? ZREVRANK<T>(string key, T member, Order order = Order.Descending)
        {
            return this.database.SortedSetRank(key, RedisExtensions.ToRedisValue(member), order);
        }

        /// <summary>
        /// ZSCORE key member
        /// 返回有序集 key 中，成员 member 的 score 值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <returns>member 成员的 score 值，以字符串形式表示</returns>
        public double? ZSCORE<T>(string key, T member)
        {
            return this.database.SortedSetScore(key, RedisExtensions.ToRedisValue(member));
        }

        /// <summary>
        /// ZUNIONSTORE destination key [key ...] [WEIGHTS weight] [AGGREGATE SUM|MIN|MAX]
        /// ZUNIONSTORE 命令将给定的一个或多个有序集的并集存储在指定的有序集 destination 中。
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="weight1"></param>
        /// <param name="weight2"></param>
        /// <param name="aggregate"></param>
        /// <returns>保存到 destination 的结果集的成员数量</returns>
        public long ZUNIONSTORE(string destination, string first, string second, double weight1 = 1, double weight2 = 1, Aggregate aggregate = Aggregate.Sum)
        {
            return this.database.SortedSetCombineAndStore(SetOperation.Union, destination, new RedisKey[] { first, second }, new double[] { weight1, weight2 }, aggregate);
        }

        #endregion

    }
}