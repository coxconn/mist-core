using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace System
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 生成时间戳，自1970年1月1日 0点0分0秒以来的毫秒数
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GenerateTimeStamp(this DateTime date)
        {
            TimeSpan ts = date - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }

        /// <summary>
        /// 时间友好化显示
        /// </summary>
        /// <param name="time"></param>
        /// <param name="curr"></param>
        /// <returns></returns>
        public static string GetFriendlyTime(this DateTime time, DateTime? curr = null)
        {
            TimeSpan ts = (curr ?? DateTime.Now) - time;

            if (ts.TotalDays >= 365)
            {
                return time.Year + "年";
            }

            if (DateTime.Now.Year > time.Year)
            {
                var totalDays = (DateTime.Now - time).TotalDays;
                if (totalDays >= 30)
                {
                    return Convert.ToInt32((totalDays / 30)) + "个月前";
                }
                else if (totalDays >= 15)
                {
                    return "半个月前";
                }
                else if (totalDays > 1)
                {
                    return Convert.ToInt32(totalDays) + "天前";
                }
                else
                {
                    return "昨天";
                }
            }

            if (((DateTime.Now.Day - time.Day) >= 2 && DateTime.Now.Year == time.Year && DateTime.Now.Month == time.Month) || (DateTime.Now.Year == time.Year && DateTime.Now.Month > time.Month))
            {
                return time.Month + "月" + time.Day + "日";
            }

            if ((DateTime.Now.Day - time.Day) > 0 && (DateTime.Now.Day - time.Day) < 2)
            {
                return "昨天";
            }

            if (ts.TotalHours >= 10 && DateTime.Now.Day == time.Day)
            {
                return "今天";
            }

            if (ts.TotalHours >= 1 && ts.TotalHours < 10 && DateTime.Now.Day == time.Day)
            {
                return (int)ts.TotalHours + "小时前";
            }

            if (ts.TotalMinutes >= 5 && ts.TotalMinutes <= 59 && DateTime.Now.Day == time.Day)
            {
                return ((int)ts.TotalMinutes).ToString() + "分钟前";
            }
            return "刚刚";
        }

    }
}