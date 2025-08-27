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
        public DateTime LastTime { get; set; }

        public int? SlidingMillisecond { get; set; }

        public DateTime? AbsExpireTime { get; set; }

        public bool IsExpire { get; set; }

        public T Item { get; set; }
    }
}
