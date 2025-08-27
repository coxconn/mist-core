using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Text;

namespace MistCore.Framework.Text
{
    public class GZipCompression
    {
        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Compress(string data)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    gzipStream.Write(buffer, 0, buffer.Length);
                }
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        /// <summary>
        /// 解压
        /// </summary>
        /// <param name="compressedData"></param>
        /// <returns></returns>
        public string Decompress(string compressedData)
        {
            byte[] buffer = Convert.FromBase64String(compressedData);
            using (var memoryStream = new MemoryStream(buffer))
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    using (var reader = new StreamReader(gzipStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}
