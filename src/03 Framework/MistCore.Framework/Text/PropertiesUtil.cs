using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MistCore.Framework.Text
{
    /// <summary>
    /// Properties parser
    /// </summary>
    public class PropertiesUtil
    {
        /// <summary>
        /// Propertieses the parser.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public Dictionary<string, string> Parser(string filePath)
        {
            Dictionary<string, string> map;
            using (System.IO.StreamReader sr = new System.IO.StreamReader(filePath))
            {
                map = new Dictionary<string, string>();
                string str = string.Empty;
                string key = string.Empty;
                string value = string.Empty;
                while ((str = sr.ReadLine()) != null)
                {
                    key = str.Substring(0, str.IndexOf('='));
                    value = str.Substring(str.IndexOf('=') + 1);
                    map.Add(key, value);
                }
            }
            return map;
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public string SerializeObject(object entity)
        {
            FieldInfo[] pis = entity.GetType().GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            StringBuilder sb = new StringBuilder();
            foreach (FieldInfo property in pis)
            {
                try
                {
                    sb.AppendLine(string.Format("{0}={1}", property.Name, property.GetValue(entity)));
                }
                catch (Exception)
                {
                    //do nothing
                }
            }
            return sb.ToString();
        }

    }
}
