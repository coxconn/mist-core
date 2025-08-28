using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MistCore.Core.AspNet.Modules;
using System;
using System.Linq;
using System.Reflection;

namespace MistCore.Framework.Net
{
    /// <summary>
    /// ModuleInitializer
    /// </summary>
    public class ModuleInitializer : IModuleAspNetInitializer
    {

        public void ConfigureServices(IServiceCollection services)
        {

            //var serivceTypes = Assembly.GetAssembly(this.GetType()).GetTypes().Where(t => typeof(IApplicationService).IsAssignableFrom(t) && !t.GetTypeInfo().IsAbstract);
            //foreach (var serviceType in serivceTypes)
            //{
            //    foreach (var intfType in serviceType.GetInterfaces())
            //    {
            //        services.AddTransient(intfType, serviceType);
            //    }
            //}


            //ÓÊÏä·þÎñ
            services.AddSingleton(typeof(MailClient), sp =>
            {
                var configuration = sp.GetService<IConfiguration>();

                var mailServer = new MailServer();
                configuration.GetSection("MailServer").Bind(mailServer);

                if (mailServer == null || mailServer.Host == null)
                {
                    throw new ArgumentNullException(string.Format("MailServer configuration is null: {0}", mailServer));
                }
                var mailClient = new MailClient(mailServer);
                return mailClient;
            });

        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {

        }

    }
}

