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
    public class MinioInfo
    {
        public string Endpoint { get; set; }
        public string AccessKey { get; set; } = "";
        public string SecretKey { get; set; } = "";
        public string Region { get; set; } = "";
        public string SessionToken { get; set; } = "";
        public string Bucket { get; set; }
        public string Download { get; set; }

    }
}
