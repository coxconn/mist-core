using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace MistCore.Core.Modules
{
    /// <summary>
    /// IModuleInitializer
    /// </summary>
    public interface IModuleInitializer
    {
        /// <summary>
        /// ConfigureServices
        /// </summary>
        /// <param name="serviceCollection">服务注册服务集合</param>
        void ConfigureServices(IServiceCollection serviceCollection);
    }


}
