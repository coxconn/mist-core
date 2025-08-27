using Microsoft.Extensions.DependencyInjection;
using MistCore.Core.Modules;
using MistCore.Data.EFProvider.Entity;

namespace MistCore.Data.EFProvider
{
    /// <summary>
    /// ModuleInitializer
    /// </summary>
    public class ModuleInitializer : IModuleInitializer
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

            services.AddTransient(typeof(IRepository<,>), typeof(EntityRepository<,>));
            services.AddTransient(typeof(IRepository<,,>), typeof(EntityRepositoryRWSplitting<,,>));

        }
    }
}

