using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace MistCore.Framework.Cache
{
    internal class CacheItem<T>
    {
        public DateTime? ExpireTime { get; set; }

        public T Item { get; set; }
    }
}
