using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using MistCore.Core.Modules;

namespace MistCore.Core.AspNet.Modules
{
    /// <summary>
    /// IModuleInitializer
    /// </summary>
    public interface IModuleAspNetInitializer : IModuleInitializer
    {

        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        void Configure(IApplicationBuilder app, IHostEnvironment env);
    }

}
