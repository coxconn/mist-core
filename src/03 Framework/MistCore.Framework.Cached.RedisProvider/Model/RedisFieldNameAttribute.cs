using System;
using System.Collections.Generic;
using System.Text;

namespace MistCore.Framework.Cached.RedisProvider
{

    [AttributeUsage(AttributeTargets.Property)]
    public class RedisFieldNameAttribute : Attribute
    {
        public string Name { get; }
        public RedisFieldNameAttribute(string name) => Name = name;
    }

}
