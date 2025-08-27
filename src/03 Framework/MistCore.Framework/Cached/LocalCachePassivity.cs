using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MistCore.Framework.Cached
{

    /// <summary>
    /// 被动性缓存，不会主动删除缓存
    /// </summary>
    public class LocalCachePassivity: ICache
    {
        private readonly static ConcurrentDictionary<string, Lazy<CacheItemPassivity<object>>> ipools = new ConcurrentDictionary<string, Lazy<CacheItemPassivity<object>>>();

        public static int Count { get { return ipools.Count; } }

        public string Get(string key)
        {
            return Get<string>(key);
        }

        public T Get<T>(string key)
        {
            Lazy<CacheItemPassivity<object>> value;
            if (ipools.TryGetValue(key, out value))
            {
                return (T)value.Value.Item;
            }
            return (T)default;
        }

        public bool Remove(string key)
        {
            Lazy<CacheItemPassivity<object>> tmp;
            return ipools.TryRemove(key, out tmp);
        }

        public bool Exist(string key)
        {
            return ipools.ContainsKey(key);
        }

        public void Refresh(string key)
        {
            Lazy<CacheItemPassivity<object>> value;
            if (ipools.TryGetValue(key, out value))
            {
                value.Value.LastTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="slidingMillisecond"></param>
        /// <param name="absoluteTime"></param>
        public void Set<T>(string key, T value, int? slidingMillisecond = null, DateTime? absoluteTime = null)
        {
            var item = ipools.AddOrUpdate(key,
                key => new Lazy<CacheItemPassivity<object>>(() =>
                {
                    var item = value;
                    var ch = new CacheItemPassivity<object>();
                    ch.Item = item;
                    ch.SlidingMillisecond = slidingMillisecond;
                    ch.AbsExpireTime = absoluteTime;
                    ch.LastTime = DateTime.Now;
                    return ch;
                }),
                (key, item) =>
                {
                    item.Value.Item = value;
                    item.Value.SlidingMillisecond = slidingMillisecond;
                    item.Value.AbsExpireTime = absoluteTime;
                    item.Value.LastTime = DateTime.Now;
                    return item;
                }
            );

        }

        /// <summary>
        /// GetOrAdd
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="slidingMillisecond"></param>
        /// <param name="absoluteTime"></param>
        public T GetOrAdd<T>(string key, Func<string, T> func, int? slidingMillisecond = null, DateTime? absoluteTime = null)
        {
            var item = ipools.GetOrAdd(key,
                key => new Lazy<CacheItemPassivity<object>>(() =>
                {
                    var item = func(key);
                    var ch = new CacheItemPassivity<object>();
                    ch.Item = item;
                    ch.SlidingMillisecond = slidingMillisecond;
                    ch.AbsExpireTime = absoluteTime;
                    ch.LastTime = DateTime.Now;
                    return ch;
                })
            );

            var upstatus = 0;
            var value = item.Value;

            if (value.SlidingMillisecond != null && (DateTime.Now - value.LastTime).TotalMilliseconds > value.SlidingMillisecond)
            {
                value.SlidingMillisecond = slidingMillisecond;
                value.AbsExpireTime = absoluteTime;
                upstatus = 2;
            }
            else if (value.AbsExpireTime != null && DateTime.Now > value.AbsExpireTime)
            {
                value.SlidingMillisecond = slidingMillisecond;
                value.AbsExpireTime = absoluteTime;
                upstatus = 2;
            }

            value.LastTime = DateTime.Now;

            if (upstatus == 2)
            {
                ThreadPool.QueueUserWorkItem((obj) =>
                {
                    var state = (KeyValuePair<string, CacheItemPassivity<object>>)obj;
                    var item = state.Value.Item;
                    lock (state.Value)
                    {
                        if (item == state.Value.Item)
                        {
                            state.Value.Item = func(state.Key);
                        }
                    }
                }, new KeyValuePair<string, CacheItemPassivity<object>>(key, value));
            }

            return (T)value.Item;
        }

    }
}
