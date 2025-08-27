using MistCore.Core.Modules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MistCore.Core.ConfigurationManager
{
    /// <summary>
    /// DomainModuleConfigurationManager
    /// </summary>
    internal class DomainModuleConfigurationManager : IModuleConfigurationManager
    {
        private AppDomain modulesDomain;

        public DomainModuleConfigurationManager():this(AppDomain.CurrentDomain)
        {
        }

        public DomainModuleConfigurationManager(AppDomain domain)
        {
            modulesDomain = domain;
        }

        /// <summary>
        /// 获取域所有程序集
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ModuleInfo> GetModules()
        {
            var types = modulesDomain.GetAssemblies()
                .SelectMany(c => c.GetTypes().Where(t => typeof(IModuleInitializer).IsAssignableFrom(t) && t != typeof(IModuleInitializer)));

            var modules = types.Select(c => new ModuleInfo
            {
                Id = c.AssemblyQualifiedName,
                Name = c.FullName,
                Type = c,
            }).ToList();

            return modules ?? new List<ModuleInfo>();
        }
    }
}
