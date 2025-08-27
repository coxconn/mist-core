using MistCore.Core.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MistCore.Core.ConfigurationManager
{
    internal class FileModuleConfigurationManager : IModuleConfigurationManager
    {
        private string modulesConfigFilePath;

        public FileModuleConfigurationManager():this("modules.json")
        {
        }

        public FileModuleConfigurationManager(string filepath)
        {
            modulesConfigFilePath = filepath;
        }

        /// <summary>
        /// 获取配置文件的程序集
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ModuleInfo> GetModules()
        {
            List<ModuleInfo> modules = new List<ModuleInfo>();

            var modulesPath = Path.Combine(GlobalConfiguration.ContentRootPath, modulesConfigFilePath);

            if (!File.Exists(modulesPath))
            {
                return modules;
            }

            using (var reader = new StreamReader(modulesPath))
            {
                string content = reader.ReadToEnd();
                modules = JsonSerializer.Deserialize<List<ModuleInfo>>(content);
            }
            return modules ?? new List<ModuleInfo>();
        }
    }
}
