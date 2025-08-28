using Microsoft.AspNetCore.Http;
using Minio;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Minio.Exceptions;

namespace MistCore.Framework.Minio
{
    public class FileManager
    {
        private string download = "";
        private string bucketName = "test";//默认桶

        private MinioClient client;

        public FileManager(MinioInfo minioInfo)
        {
            this.client = new MinioClient(minioInfo.Endpoint, minioInfo.AccessKey, minioInfo.SecretKey, minioInfo.Region, minioInfo.SessionToken);
            this.bucketName = minioInfo.Bucket ?? this.bucketName;
            this.download = minioInfo.Download;
        }

        public async Task<string> UploadFile(byte[] data, string fileName, string contentType)
        {
            string objectName = $"{DateTime.Now:yyyy/MM}/{fileName}";

            using (var stream = new MemoryStream(data))
            {
                stream.Position = 0;
                await client.PutObjectAsync(bucketName,
                    objectName,
                    stream,
                    data.Length,
                    contentType);
            }

            return download + "/" + bucketName + "/" + objectName;
        }

        public async Task<List<string>> UploadFile(IFormFileCollection files)
        {
            long size = files.Sum(f => f.Length);
            //bool found = await client.BucketExistsAsync(bucketName);
            //if (!found)
            //{
            //    await client.MakeBucketAsync(bucketName);
            //}

            var fileNames = files.Select(async formFile =>
            {
                if (formFile.Length == 0)
                {
                    return null;
                }

                Stream stream = formFile.OpenReadStream();

                #region MD5 sign
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(stream);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                var sign = sb.ToString();
                #endregion

                stream.Position = 0;

                //string saveFileName = $"{Guid.NewGuid():N}{Path.GetExtension(formFile.FileName)}";
                string saveFileName = $"{sign.ToLower()}{Path.GetExtension(formFile.FileName)}";
                string objectName = $"{DateTime.Now:yyyy/MM}/{saveFileName}";

                await client.PutObjectAsync(bucketName,
                    objectName,
                    stream,
                    formFile.Length,
                    formFile.ContentType);

                return download + "/" + bucketName + "/" + objectName;
            })
            .Select(c => c.Result)
            .ToList();

            return fileNames;
        }

        public async Task<MemoryStream> DownloadFile(string fileName)
        {
            var memoryStream = new MemoryStream();

            await client.StatObjectAsync(bucketName, fileName);
            await client.GetObjectAsync(bucketName, fileName,
                (stream) =>
                {
                    stream.CopyTo(memoryStream);
                });
            memoryStream.Position = 0;

            return memoryStream;
        }

        private static string GetContentType(string fileName)
        {
            if (fileName.Contains(".jpg"))
            {
                return "image/jpg";
            }
            else if (fileName.Contains(".jpeg"))
            {
                return "image/jpeg";
            }
            else if (fileName.Contains(".png"))
            {
                return "image/png";
            }
            else if (fileName.Contains(".gif"))
            {
                return "image/gif";
            }
            else if (fileName.Contains(".pdf"))
            {
                return "application/pdf";
            }
            else
            {
                return "application/octet-stream";
            }
        }


    }
}
