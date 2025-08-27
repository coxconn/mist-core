using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace MistCore.Framework.Cache
{
    internal class CacheItemLong<T>
    {
        public DateTime? SilpExpireTime { get; set; }

        public DateTime? AbsExpireTime { get; set; }

        public T Item { get; set; }
    }
}
