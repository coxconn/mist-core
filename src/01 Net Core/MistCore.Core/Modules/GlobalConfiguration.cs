using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MistCore.Core.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MistCore.Core.Modules
{
    /// <summary>
    /// GlobalConfiguration
    /// </summary>
    public static partial class GlobalConfiguration
    {
        /// <summary>
        /// Modules
        /// </summary>
        public static IList<ModuleInfo> Modules { get; internal set; } = new List<ModuleInfo>();

        /// <summary>
        /// DefaultCulture
        /// </summary>
        public static string DefaultCulture { get; internal set; } = "zh_CN";

        /// <summary>
        /// EnvironmentName
        /// </summary>
        public static string EnvironmentName { get; internal set; }

        /// <summary>
        /// ContentRootPath
        /// </summary>
        public static string ContentRootPath { get; internal set; }

        /// <summary>
        /// Configuration
        /// </summary>
        public static IConfiguration Configuration { get; internal set; }

        /// <summary>
        /// 应用生命周期
        /// </summary>
        public static IHostApplicationLifetime Lifetime { get; internal set; }

        /// <summary>
        /// 当前运行项目IP
        /// </summary>
        public static string IP { get; set; }

        /// <summary>
        /// 当前运行项目Port
        /// </summary>
        public static int Port { get; set; }

        /// <summary>
        /// 用于提供服务
        /// </summary>
        public static IServiceLocator ServiceLocator { get; internal set; }

    }
}
