using System;
using System.Collections.Generic;
using System.Text;

namespace MistCore.Framework.Text
{
    public class TextStartUtil
    {
        /// <summary>
        /// Replaces the star.
        /// 字串打码
        /// </summary>
        /// <param name="oldstr">The oldstr.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="replaceConut">The replace conut.</param>
        /// <param name="starCount">The star count.</param>
        /// <returns></returns>
        public string ReplaceStar(string oldstr, int startIndex, int replaceConut, int? starCount = null)
        {
            if (string.IsNullOrEmpty(oldstr))
            {
                return string.Empty;
            }
            if (oldstr.Length - 1 < startIndex)
            {
                return oldstr;
            }

            int leng = startIndex + replaceConut > oldstr.Length ? oldstr.Length : startIndex + replaceConut;
            int count = leng - startIndex;

            if (starCount == null)
            {
                starCount = count;
            }

            var starChars = string.Empty;
            for (var i = 0; i < starCount; i++)
            {
                starChars += "*";
            }
            var newChars = oldstr.Remove(startIndex, count).Insert(startIndex, starChars);

            return newChars;
        }

        /// <summary>
        /// 邮箱地址打码
        /// </summary>
        /// <param name="mail"></param>
        /// <returns></returns>
        public string MailMakeStar(string mail)
        {
            var indexOf = 0;
            if ((indexOf = mail.LastIndexOf('@')) == -1)
            {
                return string.Empty;
            }
            if (indexOf <= 2)
            {
                return mail;
            }

            return mail.Substring(0, 2) + "****" + mail.Substring(indexOf, mail.Length - indexOf);
        }

    }
}
