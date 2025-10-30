using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace MistCore.Framework.Cached
{
    internal class CacheItemPassivity<T>
    {
        public DateTime LastQueryTime { get; set; }

        public DateTime LastMarkTime { get; set; }

        public TimeSpan? SlidingTimeSpan { get; set; }

        public TimeSpan? AbsExpireTimeSpan { get; set; }

        public bool IsExpire { get; set; }

        public bool isError { get; set; }

        public T Item { get; set; }
    }
}
