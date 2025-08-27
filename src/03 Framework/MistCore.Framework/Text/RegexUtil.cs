using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MistCore.Framework.Text
{
    public class RegexUtil
    {
        /// <summary>
        /// 是否IP
        /// </summary>
        /// <param name="str1"></param>
        /// <returns></returns>
        public bool IsIPAddress(string str1)
        {
            if (str1 == null || str1 == string.Empty || str1.Length < 7 || str1.Length > 15) return false;
            string regformat = @"^\\d{1,3}[\\.]\\d{1,3}[\\.]\\d{1,3}[\\.]\\d{1,3}$";
            Regex regex = new Regex(regformat, RegexOptions.IgnoreCase);
            return regex.IsMatch(str1);
        }

        /// <summary>
        /// 是否MAIL
        /// </summary>
        /// <param name="str1"></param>
        /// <returns></returns>
        public bool IsMail(string str1)
        {
            if (str1 == null || str1 == string.Empty) return false;
            string regformat = @"^[a-zA-Z0-9_.-]+@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*\.[a-zA-Z0-9]{2,6}$";
            Regex regex = new Regex(regformat, RegexOptions.IgnoreCase);
            return regex.IsMatch(str1);
        }

        /// <summary>
        /// 验证手机号码
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public bool IsPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            string regformat = @"^((13[0-9])|(17[0-9])|(14[0-9])|(19([0-9]))|(16([0-9]))|(15([0-9]))|(18[0-9]))+\d{8}$";
            Regex regex = new Regex(regformat, RegexOptions.IgnoreCase);
            return regex.IsMatch(phone);
        }

        /// <summary>
        /// 验证手机号码
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public bool IsPhoneHongKong(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            string regformat = @"^1[3|4|5|6|7|8|9][0-9]+\d{8}$|^([1-9])+\d{7}$|^[6|5]([8|6])+\d{5}$";
            Regex regex = new Regex(regformat, RegexOptions.IgnoreCase);
            return regex.IsMatch(phone);
        }

        /// <summary>
        /// 密码复杂度
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool IsPassword(string password)
        {
            if (password == null || password == string.Empty || password.Length < 6 || password.Length > 20) return false;
            string regformat = @"^(?!^\d+$)(?!^[a-zA-Z]+$)[0-9a-zA-Z]{6,20}$";
            Regex regex = new Regex(regformat, RegexOptions.IgnoreCase);
            return regex.IsMatch(password);
        }

        /// <summary>
        /// 账号复杂度
        /// 以英文字母开头，只能包含英文字母、数字、下划线
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public bool IsLegitimateUserName(string username)
        {
            if (username == null || username == string.Empty) return false;
            string regformat = @"^[a-zA-Z][a-zA-Z0-9_]*$";
            Regex regex = new Regex(regformat, RegexOptions.IgnoreCase);
            return regex.IsMatch(username);
        }
    }
}
