using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace MistCore.Framework.Cache
{
    /// <summary>
    /// LocalCache
    /// </summary>
    public class LocalCache
    {
        private readonly static ConcurrentDictionary<string, Lazy<CacheItem<object>>> ipools = new ConcurrentDictionary<string, Lazy<CacheItem<object>>>();

        private static BackgroundThreadService polling;
        private static readonly int keepSleep = 5 * 60 * 1000;
        private static readonly object lockcc = new object();

        static LocalCache()
        {
            polling = new BackgroundThreadService(() =>
            {
                try
                {
                    Iterator();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("iteration error." + ex.Message);
                }

                Thread.Sleep(keepSleep);
            });
            polling.Start();

            ThreadPool.SetMinThreads(250, 250);
        }

        private static void Iterator()
        {
            lock (lockcc)
            {
                var rlist = ipools.Where(c => c.Value.Value.ExpireTime < DateTime.Now).ToList();

                rlist.ForEach(item =>
                {
                    Lazy<CacheItem<object>> tmp;
                    ipools.TryRemove(item.Key, out tmp);
                });
            }
        }

        public static int Count { get { return ipools.Count; } }

        public static T GetOrAdd<T>(string key, Func<string, T> func, TimeSpan? timeSpan = null)
        {
            var ipdns = ipools.GetOrAdd(key,
                key => new Lazy<CacheItem<object>>(() =>
                {
                    var item = func(key);
                    var ch = new CacheItem<object>();
                    ch.Item = item;
                    if (timeSpan != null)
                    {
                        ch.ExpireTime = DateTime.Now.AddMilliseconds(timeSpan.Value.TotalMilliseconds);
                    }
                    return ch;
                })
            );

            polling.KeepAlive();

            return (T)ipdns?.Value?.Item;
        }

        public static T Get<T>(string key)
        {
            Lazy<CacheItem<object>> value;
            if (ipools.TryGetValue(key, out value))
            {
                return (T)value.Value.Item;
            }
            return (T)default;
        }

        public static bool Remove(string key)
        {
            Lazy<CacheItem<object>> tmp;
            return ipools.TryRemove(key, out tmp);
        }

    }
}
