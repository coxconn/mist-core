using MistCore.Data;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MistCore.Framework.EService
{

    public static class ESClientExtension
    {
        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q1"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static SearchDescriptor<T> Pagination<T>(this SearchDescriptor<T> q1, PageInfo page) where T : class
        {
            if (page == null)
            {
                return q1;
            }

            if (page.Take > 0)
            {
                q1.From(page.Skip).Take(page.Take);
            }
            else if (page.PageNo > 0)
            {
                var index = (page.PageNo - 1) * page.PageSize;
                q1.From(index).Take(page.PageSize);
            }
            else
            {
                q1.From(0).Take(0);
            }

            if (!string.IsNullOrEmpty(page.Order))
            {
                var orders = page.Order
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    .Where(c => c.Length >= 1 && !string.IsNullOrEmpty(c[0]))
                    .Select(c => new { sort = c[0], isDesc = c.Length > 1 ? "desc".Equals(c[1], StringComparison.OrdinalIgnoreCase) : false })
                    .ToList();
                var sortDescriptor = new SortDescriptor<T>();
                foreach (var order in orders)
                {
                    if (order.isDesc)
                    {
                        sortDescriptor.Descending(order.sort);
                    }
                    else
                    {
                        sortDescriptor.Ascending(order.sort);
                    }
                }

                if (orders.Count > 0)
                {
                    q1.Sort(st => sortDescriptor);
                }
            }
            //else if (!string.IsNullOrEmpty(page.Sort))
            //{
            //    if (page.isDesc)
            //    {
            //        q1.Sort(st => st.Ascending(page.sort));
            //    }
            //    else
            //    {
            //        q1.Sort(st => st.Descending(page.sort));
            //    }
            //}
            return q1;
        }

        /// <summary>
        /// 高亮字段简写
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q1"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static IHighlight Fields<T>(this HighlightDescriptor<T> q1, params Expression<Func<T, object>>[] fields) where T : class
        {
            IHighlight highlight = q1;

            if (highlight.PreTags == null || highlight.PreTags.Count() == 0)
            {
                highlight.PreTags = new List<string> { "<em color='red'>" };
            }

            if (highlight.PostTags == null || highlight.PostTags.Count() == 0)
            {
                highlight.PostTags = new List<string> { "</em>" };
            }

            if (fields != null && fields.Length > 0)
            {
                highlight.Fields = highlight.Fields ?? new Dictionary<Field, IHighlightField>();
                var highlightFieldDescriptor = new HighlightFieldDescriptor<T>();

                foreach (var fc in fields)
                {
                    var field = new Field(fc);
                    var highlightField = highlightFieldDescriptor.Field(field);
                    highlight.Fields[field] = highlightField;
                }
            }
            return highlight;
        }

        /// <summary>
        /// 处理查询结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rsp"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static List<T> Assignment<T>(this ISearchResponse<T> rsp, PageInfo page = null) where T : class
        {
            if (page != null)
            {
                page.Total = rsp.Total;
                if (page.PageSize != 0)
                {
                    page.PageCount = (int)Math.Ceiling(page.Total / (double)page.PageSize);
                }
            }

            var fields = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            return rsp.Hits.Select(c =>
            {
                var item = c.Source;
                if (c.Highlights == null)
                {
                    return item;
                }
                foreach (var highlights in c.Highlights)
                {
                    var field = fields.Where(l => highlights.Value.Field.Equals(l.Name, StringComparison.OrdinalIgnoreCase) || highlights.Value.Field.StartsWith(l.Name + ".", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (field == null)
                    {
                        continue;
                    }

                    if (field.PropertyType.IsGenericType && field.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        field.SetValue(item, highlights.Value.Highlights.ToList());
                    }
                    else if (field.PropertyType == typeof(string))
                    {
                        field.SetValue(item, highlights.Value.Highlights.FirstOrDefault());
                    }
                }
                return item;
            }).ToList();
        }

        /// <summary>
        /// Terms 手动分割
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q1"></param>
        /// <param name="selector"></param>
        /// <param name="split">分割符 默认：,</param>
        /// <returns></returns>
        public static QueryContainer TermSplit<T>(this QueryContainerDescriptor<T> q1, Func<TermsQueryDescriptor<T>, ITermsQuery> selector, string split = ",") where T : class
        {
            var tq = new TermsQueryDescriptor<T>();
            var st = selector(tq);

            var q2 = new QueryContainerDescriptor<T>();

            if (st.Terms == null)
            {
                return q2;
            }

            if (st.Terms.FirstOrDefault()?.GetType() != typeof(string))
            {
                return q2.Terms(t => st);
            }

            var keys = st.Terms.OfType<string>().SelectMany(c => c.Split(split.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Distinct()).ToArray();
            if (keys.Length > 1)
            {
                st.Terms = keys;
                return q2.Terms(t => st);
            }

            return q2.Term(t => t.Field(st.Field).Value(keys.FirstOrDefault()).Boost(st.Boost).Name(st.Name).Strict(st.IsStrict).Verbatim(st.IsVerbatim));
        }

        /// <summary>
        /// Presix 手动分割
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q1"></param>
        /// <param name="selector"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static QueryContainer PrefixSplit<T>(this QueryContainerDescriptor<T> q1, Func<PrefixQueryDescriptor<T>, IPrefixQuery> selector, string split = ",") where T : class
        {
            var pq = new PrefixQueryDescriptor<T>();
            var st = selector(pq);

            var q2 = new QueryContainerDescriptor<T>();

            if (st.Value == null)
            {
                return q2;
            }

            if (st.Value?.GetType() != typeof(string))
            {
                return q2.Prefix(f => st);
            }

            var keys = (st.Value as string).Split(split.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
            if (keys.Length > 1)
            {
                var qc = new QueryContainer();

                foreach (var key in keys)
                {
                    qc |= q2.Prefix(f => f.Value(key).Field(st.Field).Boost(st.Boost).Name(st.Name).Strict(st.IsStrict).Verbatim(st.IsVerbatim));
                }
                q2.Bool(c => c.Should(f => qc));
                return qc;
            }

            return q2.Prefix(f => st);
        }

        /// <summary>
        /// TermRange 手动分割
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q1"></param>
        /// <param name="selector"></param>
        /// <param name="split">分割符 默认：,</param>
        /// <returns></returns>
        public static QueryContainer TermRangeSplit<T>(this QueryContainerDescriptor<T> q1, Func<TermRangeSplitQueryDescriptor<T>, ITermRangeSplitQuery> selector, string split = ",") where T : class
        {
            var tq = new TermRangeSplitQueryDescriptor<T>();
            var st = selector(tq);

            var q2 = new QueryContainerDescriptor<T>();

            if (st.Ranges == null)
            {
                return q2;
            }

            var keys = (st.Ranges as string).Split(split.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
            if (keys.Length > 1)
            {
                var qc = new QueryContainer();

                foreach (var key in keys)
                {
                    qc |= q2.TermRange(f => f.Field(st.Field).Boost(st.Boost).Name(st.Name).Strict(st.IsStrict).Verbatim(st.IsVerbatim).Range(key, st.Split));
                }
                q2.Bool(c => c.Should(f => qc));
                return qc;
            }

            return q2.TermRange(f => f.Field(st.Field).Boost(st.Boost).Name(st.Name).Strict(st.IsStrict).Verbatim(st.IsVerbatim).Range(st.Ranges, st.Split));
        }
        public interface ITermRangeSplitQuery : IRangeQuery, IFieldNameQuery, IQuery
        {
            string Ranges { get; set; }
            string Split { get; set; }
        }
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public class TermRangeSplitQueryDescriptor<T> : FieldNameQueryDescriptorBase<TermRangeSplitQueryDescriptor<T>, ITermRangeSplitQuery, T>, ITermRangeSplitQuery, IRangeQuery, IFieldNameQuery, IQuery where T : class
        {

            protected override bool Conditionless { get; }
            public string Ranges { get; set; }
            public string Split { get; set; }

            /// <summary>
            /// 数值范围取词
            /// -1 代表-1
            /// 2-5 代表2-5
            /// 5- 代表5-
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="q1"></param>
            /// <param name="value"></param>
            /// <param name="split">分割符 默认：-</param>
            /// <returns></returns>
            public TermRangeSplitQueryDescriptor<T> Range(string value, string split = "-")
            {
                this.Ranges = value;
                this.Split = split;
                return this;
            }
        }


        /// <summary>
        /// DateRange 手动分割
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q1"></param>
        /// <param name="selector"></param>
        /// <param name="split">分割符 默认：,</param>
        /// <returns></returns>
        public static QueryContainer DateRangeSplit<T>(this QueryContainerDescriptor<T> q1, Func<DateRangeSplitQueryDescriptor<T>, IDateRangeSplitQuery> selector, string split = ",") where T : class
        {
            var tq = new DateRangeSplitQueryDescriptor<T>();
            var st = selector(tq);

            var q2 = new QueryContainerDescriptor<T>();

            if (st.Ranges == null)
            {
                return q2;
            }

            var keys = (st.Ranges as string).Split(split.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
            if (keys.Length > 1)
            {
                var qc = new QueryContainer();

                foreach (var key in keys)
                {
                    qc |= q2.DateRange(f => f.Field(st.Field).Boost(st.Boost).Name(st.Name).Strict(st.IsStrict).Verbatim(st.IsVerbatim).Range(key, st.Split));
                }
                q2.Bool(c => c.Should(f => qc));
                return qc;
            }

            return q2.DateRange(f => f.Field(st.Field).Boost(st.Boost).Name(st.Name).Strict(st.IsStrict).Verbatim(st.IsVerbatim).Range(st.Ranges, st.Split));
        }
        public interface IDateRangeSplitQuery : IRangeQuery, IFieldNameQuery, IQuery
        {
            string Ranges { get; set; }
            string Split { get; set; }
        }
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public class DateRangeSplitQueryDescriptor<T> : FieldNameQueryDescriptorBase<DateRangeSplitQueryDescriptor<T>, IDateRangeSplitQuery, T>, IDateRangeSplitQuery, IRangeQuery, IFieldNameQuery, IQuery where T : class
        {
            protected override bool Conditionless { get; }
            public string Ranges { get; set; }
            public string Split { get; set; }

            /// <summary>
            /// 时间范围取词
            /// -1 代表1年内
            /// 2-5 代表2-5年
            /// 5- 代表5年以上
            /// 2021-2023
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="q1"></param>
            /// <param name="value"></param>
            /// <param name="split">分割符 默认：-</param>
            /// <param name="format">日期格式 默认：yyyyMMddHHmmss</param>
            /// <returns></returns>
            public DateRangeSplitQueryDescriptor<T> Range(string value, string split = "-")
            {
                this.Ranges = value;
                this.Split = split;
                return this;
            }
        }

        /// <summary>
        /// 时间范围取词
        /// -1 代表1年内
        /// 2-5 代表2-5年
        /// 5- 代表5年以上
        /// 2021-2023 代表2021年-2023年
        /// 1m-3m 代表过去1-3月
        /// 1d-3d 代表过去1-3天
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q1"></param>
        /// <param name="value"></param>
        /// <param name="split">分割符 默认：-</param>
        /// <param name="format">日期格式 默认：yyyyMMddHHmmss</param>
        /// <returns></returns>
        public static DateRangeQueryDescriptor<T> Range<T>(this DateRangeQueryDescriptor<T> q1, string value, string split = "-", string format = "yyyyMMddHHmmss") where T : class
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return q1;
            }

            var nf = value.Split(split.ToCharArray());
            Array.Resize(ref nf, 2);

            var dts = nf.Select((c, i) =>
            {
                if (string.IsNullOrWhiteSpace(c))
                {
                    return null;
                }
                else if (c.EndsWith('y'))
                {
                    int it;
                    return int.TryParse(c.TrimEnd('y'), out it) ? DateTime.Now.AddYears(-it).ToString("yyyy-MM-dd HH:mm:ss") : null;
                }
                else if (c.EndsWith('m'))
                {
                    int it;
                    return int.TryParse(c.TrimEnd('m'), out it) ? DateTime.Now.AddMonths(-it).ToString("yyyy-MM-dd HH:mm:ss") : null;
                }
                else if (c.EndsWith('d'))
                {
                    int it;
                    return int.TryParse(c.TrimEnd('d'), out it) ? DateTime.Now.AddDays(-it).ToString("yyyy-MM-dd HH:mm:ss") : null;

                }
                else if (c.Length >= 4)
                {
                    DateTime dt;
                    return (DateTime.TryParseExact(c, format.Substring(0, c.Length), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt)) ? dt.ToString("yyyy-MM-dd HH:mm:ss") : null;
                }
                else
                {
                    int it;
                    return int.TryParse(c, out it) ? DateTime.Now.AddYears(-it).ToString("yyyy-MM-dd HH:mm:ss") : null;
                }
            }).ToArray();

            if (nf.Max(c => (int?)c?.Length) >= 4)
            {
                return q1.GreaterThanOrEquals(dts[0]).LessThan(dts[1]).Format("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                return q1.GreaterThanOrEquals(dts[1]).LessThan(dts[0]).Format("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 数值范围取词
        /// -1 代表-1
        /// 2-5 代表2-5
        /// 5- 代表5-
        /// 2021-2023 代表2021年-2023年
        /// 1m-3m 代表过去1-3月
        /// 1d-3d 代表过去1-3天
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q1"></param>
        /// <param name="value"></param>
        /// <param name="split">分割符 默认：-</param>
        /// <returns></returns>
        public static TermRangeQueryDescriptor<T> Range<T>(this TermRangeQueryDescriptor<T> q1, string value, string split = "-") where T : class
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return q1;
            }

            var nf = value.Split(split);
            Array.Resize(ref nf, 2);

            var dts = nf.Select((c, i) =>
            {
                if (string.IsNullOrWhiteSpace(c))
                {
                    return null;
                }
                int it;
                return int.TryParse(c, out it) ? it.ToString() : null;
            }).ToArray();

            return q1.GreaterThanOrEquals(dts[0]).LessThan(dts[1]);
        }

        /// <summary>
        /// 通过输入长度适配匹配度
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q1"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static MatchQueryDescriptor<T> MinimumShouldMatch<T>(this MatchQueryDescriptor<T> q1, string value) where T : class
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return q1;
            }
            if (value.Length == 2)
            {
                return q1.MinimumShouldMatch(new MinimumShouldMatch(100));
            }
            else if (value.Length == 3 || value.Length == 4 || value.Length == 5)
            {
                return q1.MinimumShouldMatch(new MinimumShouldMatch(80));
            }
            else
            {
                return q1.MinimumShouldMatch(new MinimumShouldMatch(70));
            }
        }

    }

}
