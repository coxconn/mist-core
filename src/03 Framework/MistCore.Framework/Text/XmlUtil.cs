using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

namespace MistCore.Framework.Text
{
    /// <summary>
    /// XML Parser
    /// </summary>
    public class XmlUtil
    {

        #region Deserialize
        /// <summary>
        /// Deserializes the specified XML.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml">The XML.</param>
        /// <returns></returns>
        public T Deserialize<T>(string xml)
        {
            using (StringReader sr = new StringReader(xml))
            {
                XmlSerializer xmldes = new XmlSerializer(typeof(T));
                return (T)xmldes.Deserialize(sr);
            }
        }

        /// <summary>
        /// Deserializes the specified file information.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileInfo">The file information.</param>
        /// <returns></returns>
        public T Deserialize<T>(FileInfo fileInfo)
        {
            using (StreamReader sr = new StreamReader(fileInfo.FullName))
            {
                XmlSerializer xmldes = new XmlSerializer(typeof(T));
                return (T)xmldes.Deserialize(sr);
            }
        }
        #endregion

        #region Serialize
        /// <summary>
        /// Serializers the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public string Serializer(object entity)
        {
            string str;
            using (MemoryStream Stream = new MemoryStream())
            {
                XmlSerializer xml = new XmlSerializer(entity.GetType());
                xml.Serialize(Stream, entity);
                Stream.Position = 0;
                using (StreamReader sr = new StreamReader(Stream))
                {
                    str = sr.ReadToEnd();
                }
            }
            return str;
        }
        #endregion
    }
}
