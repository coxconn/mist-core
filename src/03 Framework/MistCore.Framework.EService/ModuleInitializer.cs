using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MistCore.Core.AspNet.Modules;
using System.Linq;
using System.Reflection;

namespace MistCore.Framework.EService
{
    /// <summary>
    /// ModuleInitializer
    /// </summary>
    public class ModuleInitializer : IModuleAspNetInitializer
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(typeof(ESClient), sp =>
            {
                var configuration = sp.GetService<IConfiguration>();

                var conn = configuration.GetSection("ESConn").Value;

                var esclient = new ESClient(conn);
                return esclient;
            });

        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {

        }
    }
}

