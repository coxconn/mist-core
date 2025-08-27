using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace System
{
    public static class EnumExtensions
    {
        private static Dictionary<string, Dictionary<string, string>> enumCache;
        private static readonly object locker = new object();

        private static Dictionary<string, Dictionary<string, string>> EnumCache
        {
            get
            {
                if (enumCache == null)
                {
                    enumCache = new Dictionary<string, Dictionary<string, string>>();
                }
                return enumCache;
            }
            set { enumCache = value; }
        }

        public static string GetDescription(this Enum en)
        {
            string enString = string.Empty;
            if (null == en) return enString;
            var type = en.GetType();
            enString = en.ToString();
            lock (locker)
            {
                if (!EnumCache.ContainsKey(type.FullName))
                {
                    var fields = type.GetFields();
                    Dictionary<string, string> temp = new Dictionary<string, string>();
                    foreach (var item in fields)
                    {
                        var attrs = item.GetCustomAttributes(typeof(DescriptionAttribute), false);
                        if (attrs.Length == 1)
                        {
                            var v = ((DescriptionAttribute)attrs[0]).Description;
                            temp.Add(item.Name, v);
                        }
                    }
                    EnumCache.Add(type.FullName, temp);
                }

            }
            if (EnumCache[type.FullName].ContainsKey(enString))
            {
                return EnumCache[type.FullName][enString];
            }
            return enString;
        }

    }
}