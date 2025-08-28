using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MistCore.Core.AspNet.Modules;
using System.Linq;
using System.Reflection;

namespace MistCore.Framework.Export
{
    /// <summary>
    /// ModuleInitializer
    /// </summary>
    public class ModuleInitializer : IModuleAspNetInitializer
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(typeof(ExcelClient), sp =>
            {
                var configuration = sp.GetService<IConfiguration>();

                var comments = configuration.GetSection("ExcelClient:Comments").Value;
                var author = configuration.GetSection("ExcelClient:Author").Value;
                var declaration = configuration.GetSection("ExcelClient:Declaration").Value;

                var client = new ExcelClient(comments, author, declaration);
                return client;
            });

        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {

        }
    }
}

