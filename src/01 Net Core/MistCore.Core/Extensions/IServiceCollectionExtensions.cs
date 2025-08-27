using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MistCore.Core;
using MistCore.Core.ConfigurationManager;
using MistCore.Core.Modules;
using MistCore.Core.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// IServiceCollection Extensions
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Add Modules
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="env"></param>
        /// <param name="option"></param>
        /// <param name="manager"></param>
        public static void AddModules(this IServiceCollection services, IConfiguration configuration = null, IHostEnvironment env = null, Action<ModuleBuilderOption> option = null, IModuleConfigurationManager manager = null)
        {
            GlobalConfiguration.DefaultCulture = CultureInfo.CurrentCulture.Name;

            env = env ?? services.BuildServiceProvider().GetService<IHostEnvironment>();
            GlobalConfiguration.ContentRootPath = env?.ContentRootPath;
            GlobalConfiguration.EnvironmentName = env?.EnvironmentName;

            GlobalConfiguration.Configuration = configuration;

            var builderOption = new ModuleBuilderOption();
            option?.Invoke(builderOption);

            //当前域
            var currentDomainModules = new DomainModuleConfigurationManager().GetModules();
            builderOption.AddModule(currentDomainModules.ToArray());

            //配置文件
            var fileConfigModules = new FileModuleConfigurationManager().GetModules();
            builderOption.AddModule(fileConfigModules.ToArray());

            //自定义
            if (manager != null)
            {
                builderOption.AddModule(manager.GetModules().ToArray());
            }

            LoadModules(services, builderOption.modules);

            //var moduleInitializers = services.BuildServiceProvider().GetServices<IModuleInitializer>();
            //foreach (var moduleInitializer in moduleInitializers)
            //{
            //    moduleInitializer.ConfigureServices(services);
            //}

            GlobalConfiguration.ServiceLocator = new ServiceSingleLocator(services.BuildServiceProvider());
        }

        /// <summary>
        /// Load Modules
        /// </summary>
        /// <param name="modules"></param>
        private static void LoadModules(IServiceCollection services, IEnumerable<ModuleInfo> modules)
        {

            foreach (var module in modules)
            {
                if (module.Type == null)
                {
                    module.Type = Type.GetType(module.Id);
                }

                if (typeof(IModuleInitializer).IsAssignableFrom(module.Type) && module.Type != typeof(IModuleInitializer) && !module.Type.IsInterface)
                {
                    GlobalConfiguration.Modules.Add(module);
                    var m = (module.Type.Assembly.CreateInstance(module.Type.FullName) as IModuleInitializer);
                    m.ConfigureServices(services);

                    services.AddSingleton(typeof(IModuleInitializer), sp => m);
                }
            }
        }
    }
}
