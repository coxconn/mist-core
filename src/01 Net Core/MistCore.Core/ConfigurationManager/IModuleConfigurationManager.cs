using MistCore.Core.Modules;
using System.Collections.Generic;

namespace MistCore.Core.ConfigurationManager
{
    public interface IModuleConfigurationManager
    {
        IEnumerable<ModuleInfo> GetModules();
    }
}