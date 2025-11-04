using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MistCore.Data
{
    [DataContract(Namespace = "http://tempuri.org/")]
    public class AggsInfo
    {
        [DataMember(Name = "Key")]
        public string Key { get; set; }

        [DataMember(Name = "Name")]
        public string Name { get; set; }

        [DataMember(Name = "Child")]
        public List<AggsInfo> Child { get; set; }

        [DataMember(Name = "Count")]
        public int Count { get; set; }

        [DataMember(Name = "Detail")]
        public object Detail { get; set; }

        public AggsInfo() { }

        public AggsInfo(string key, string name)
        {
            this.Key = key;
            this.Name = name;
        }

        public AggsInfo(string key, string name, List<AggsInfo> child)
        {
            this.Key = key;
            this.Name = name;
            this.Child = child;
        }

        public AggsInfo(string key, string name, int count)
        {
            this.Key = key;
            this.Name = name;
            this.Count = count;
        }

        public AggsInfo(string key, string name, int count, List<AggsInfo> child)
        {
            this.Key = key;
            this.Name = name;
            this.Count = count;
            this.Child = child;
        }

        public AggsInfo(string key, string name, object detail)
        {
            this.Key = key;
            this.Name = name;
            this.Detail = detail;
        }

    }

    public class AggsExtends
    {
        public static string GetDictText(List<AggsInfo> aggs, string key)
        {
            var keys = key.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var texts = GetDictText(aggs, keys);
            return texts.Aggregate(string.Empty, (t, c) => string.IsNullOrEmpty(t) ? c : $"{t},{c}");
        }

        private static List<string> GetDictText(List<AggsInfo> aggs, string[] keys)
        {
            var rst = new List<string>();
            foreach (var key in keys)
            {
                foreach (var agg in aggs)
                {
                    if (agg.Key == key)
                    {
                        rst.Add(agg.Name);
                    }
                    if (agg.Child != null && agg.Child.Count > 0)
                    {
                        var crst = GetDictText(agg.Child, keys);
                        rst.AddRange(crst);
                    }
                }
            }
            return rst;
        }
    }
}
