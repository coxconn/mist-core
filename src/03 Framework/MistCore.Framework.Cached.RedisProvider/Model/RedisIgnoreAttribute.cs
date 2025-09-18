using System;
using System.Collections.Generic;
using System.Text;

namespace MistCore.Framework.Cached.RedisProvider
{

    [AttributeUsage(AttributeTargets.Property)]
    public class RedisIgnoreAttribute : Attribute { }

}
