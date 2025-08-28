using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MistCore.Core.AspNet.Modules;

namespace MistCore.Framework.Minio
{
    /// <summary>
    /// ModuleInitializer
    /// </summary>
    public class ModuleInitializer : IModuleAspNetInitializer
    {

        public ModuleInitializer()
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton(typeof(FileManager), sp =>
            {
                var configuration = sp.GetService<IConfiguration>();

                var minioInfo = new MinioInfo();
                configuration.GetSection("MinIO").Bind(minioInfo);

                var fileManager = new FileManager(minioInfo);
                return fileManager;
            });

        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {

        }

    }
}

