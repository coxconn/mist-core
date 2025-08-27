using System;
using System.Collections.Generic;
using System.Text;

namespace MistCore.Framework.Swagger.Config
{
    /// <summary>
    /// Swagger 配置
    /// </summary>
    public class SwaggerConfig
    {
        /// <summary>
        /// Project name
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Bearer: Authorization
        /// </summary>
        public string Security { get; set; }

        /// <summary>
        /// Rquest prefix
        /// </summary>
        public string Prefix { get; set; }
    }
}
