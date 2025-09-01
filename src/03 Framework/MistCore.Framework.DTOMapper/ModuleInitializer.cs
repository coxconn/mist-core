using Microsoft.Extensions.DependencyInjection;
using MistCore.Core.DTOMapper;
using MistCore.Core.Modules;

namespace MistCore.Framework.DTOMapper
{
    /// <summary>
    /// ModuleInitializer
    /// </summary>
    public class ModuleInitializer : IModuleInitializer
    {


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(typeof(IDTOMapper), typeof(DTOMapper));
        }



    }
}

