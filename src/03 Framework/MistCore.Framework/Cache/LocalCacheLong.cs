using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace MistCore.Framework.Cache
{

    public class LocalCacheLong
    {
        private static readonly Dictionary<string, CacheItemLong<object>> c0 = new Dictionary<string, CacheItemLong<object>>();

        private static BackgroundThreadService polling;
        private static readonly int keepSleep = 5 * 60 * 1000;
        private static readonly object lockcc = new object();

        static LocalCacheLong()
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
                var rlist = c0.Where(c => c.Value.AbsExpireTime < DateTime.Now).ToList();

                rlist.ForEach(item =>
                {
                    c0.Remove(item.Key);
                });
            }
        }

        public static int Count { get { return c0.Count; } }

        public static T GetOrAdd<T>(string key, Func<string, T> func, TimeSpan? silpTimeSpan = null, TimeSpan? absTimeSpan = null)
        {
            CacheItemLong<object> value = null;
            var upstatus = 0;

            lock (lockcc)
            {
                if (!c0.ContainsKey(key))
                {
                    value = new CacheItemLong<object>();
                    value.SilpExpireTime = silpTimeSpan == null ? (DateTime?)null : DateTime.Now.AddMilliseconds(silpTimeSpan.Value.TotalMilliseconds);
                    value.AbsExpireTime = absTimeSpan == null ? (DateTime?)null : DateTime.Now.AddMilliseconds(absTimeSpan.Value.TotalMilliseconds);
                    c0[key] = value;

                    upstatus = 1;
                }
                else
                {
                    value = c0[key];

                    if (value.SilpExpireTime < DateTime.Now)
                    {
                        value.SilpExpireTime = silpTimeSpan == null ? (DateTime?)null : DateTime.Now.AddMilliseconds(silpTimeSpan.Value.TotalMilliseconds);
                        value.AbsExpireTime = absTimeSpan == null ? (DateTime?)null : DateTime.Now.AddMilliseconds(absTimeSpan.Value.TotalMilliseconds);
                        upstatus = 2;
                    }
                }
            }

            if (upstatus == 1)
            {
                value.Item = func(key);
            }
            if (upstatus == 2)
            {
                var state = new KeyValuePair<string, CacheItemLong<object>>(key, value);

                ThreadPool.QueueUserWorkItem((obj) =>
                {
                    var state = (KeyValuePair<string, CacheItemLong<object>>)obj;
                    var item = state.Value.Item;
                    lock (state.Value)
                    {
                        if (item == state.Value.Item)
                        {
                            state.Value.Item = func(state.Key);
                        }
                    }
                }, state);
            }

            polling.KeepAlive();

            return (T)value.Item;
        }

        public static T Get<T>(string key)
        {
            lock (lockcc)
            {
                CacheItemLong<object> value;
                if (c0.TryGetValue(key, out value))
                {
                    return (T)value.Item;
                }
                return (T)default;
            }
        }

        public static bool Remove(string key)
        {
            lock (lockcc)
            {
                if (c0.ContainsKey(key))
                {
                    return c0.Remove(key);
                }
                return false;
            }
        }
    }
}
