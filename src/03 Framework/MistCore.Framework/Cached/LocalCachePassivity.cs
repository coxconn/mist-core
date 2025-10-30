using Microsoft.Extensions.Logging;
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
        private readonly ILogger<LocalCachePassivity> logger;
        private readonly static ConcurrentDictionary<string, Lazy<CacheItemPassivity<object>>> ipools = new ConcurrentDictionary<string, Lazy<CacheItemPassivity<object>>>();

        public static int Count { get { return ipools.Count; } }

        public LocalCachePassivity(ILogger<LocalCachePassivity> logger)
        {
            this.logger = logger;
        }

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
                value.Value.LastQueryTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="slidingTimeSpan"></param>
        /// <param name="absoluteTimeSpan"></param>
        public void Set<T>(string key, T value, TimeSpan? slidingTimeSpan = null, TimeSpan? absoluteTimeSpan = null)
        {
            var item = ipools.AddOrUpdate(key,
                key => new Lazy<CacheItemPassivity<object>>(() =>
                {
                    var item = value;
                    var ch = new CacheItemPassivity<object>();
                    ch.Item = item;
                    ch.SlidingTimeSpan = slidingTimeSpan;
                    ch.AbsExpireTimeSpan = absoluteTimeSpan;
                    ch.LastQueryTime = DateTime.Now;
                    ch.LastMarkTime = DateTime.Now;
                    return ch;
                }),
                (key, item) =>
                {
                    item.Value.Item = value;
                    item.Value.SlidingTimeSpan = slidingTimeSpan;
                    item.Value.AbsExpireTimeSpan = absoluteTimeSpan;
                    item.Value.LastQueryTime = DateTime.Now;
                    item.Value.LastMarkTime = DateTime.Now;
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
        /// <param name="slidingTimeSpan"></param>
        /// <param name="absoluteTimeSpan"></param>
        public T GetOrAdd<T>(string key, Func<string, T> func, TimeSpan? slidingTimeSpan = null, TimeSpan? absoluteTimeSpan = null)
        {
            var item = ipools.GetOrAdd(key,
                key => new Lazy<CacheItemPassivity<object>>(() =>
                {
                    var ch = new CacheItemPassivity<object>();
                    ch.SlidingTimeSpan = slidingTimeSpan;
                    ch.AbsExpireTimeSpan = absoluteTimeSpan;
                    ch.LastQueryTime = DateTime.Now;
                    ch.LastMarkTime = DateTime.Now;

                    for (var i = 0; i < 3; i++)
                    {
                        try
                        {
                            var item = func(key);
                            ch.Item = item;
                            ch.isError = false;
                            return ch;
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"cache create error {i + 1} times.");
                        }
                    }
                    ch.isError = true;
                    return ch;
                })
            );

            var value = item.Value;

            if (value.isError && value.LastQueryTime > DateTime.Now.AddSeconds(-8))
            {
                return (T)value.Item;
            }

            var upstatus = 0;
            if (value.isError)
            {
                upstatus = 2;
            }
            else if (value.SlidingTimeSpan != null && (DateTime.Now - value.LastQueryTime) > value.SlidingTimeSpan)
            {
                upstatus = 2;
            }
            else if (value.AbsExpireTimeSpan != null && (DateTime.Now - value.LastMarkTime) > value.AbsExpireTimeSpan)
            {
                upstatus = 2;
            }

            if (value.SlidingTimeSpan != slidingTimeSpan)
            {
                value.SlidingTimeSpan = slidingTimeSpan;
            }
            if (value.AbsExpireTimeSpan != absoluteTimeSpan)
            {
                value.AbsExpireTimeSpan = absoluteTimeSpan;
            }
            value.LastQueryTime = DateTime.Now;

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
                            for (var i = 0; i < 3; i++)
                            {
                                try
                                {
                                    var uitem = func(state.Key);
                                    state.Value.Item = uitem;
                                    state.Value.LastMarkTime = DateTime.Now;
                                    return;
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, $"cache update error {i + 1} times.");
                                }
                            }
                        }
                    }
                }, new KeyValuePair<string, CacheItemPassivity<object>>(key, value));
            }

            return (T)value.Item;
        }

    }
}
