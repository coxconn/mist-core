using Microsoft.Extensions.DependencyInjection;
using MistCore.Core.Modules;

namespace MistCore.Data
{
    /// <summary>
    /// ModuleInitializer
    /// </summary>
    public class ModuleInitializer : IModuleInitializer
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient(typeof(IDomainService<>), typeof(DomainService<>));
            services.AddTransient(typeof(IDomainService<,>), typeof(DomainService<,>));

            services.AddTransient(typeof(IApplicationService<>), typeof(ApplicationService<>));
            services.AddTransient(typeof(IApplicationService<,>), typeof(ApplicationService<,>));

        }

    }
}

