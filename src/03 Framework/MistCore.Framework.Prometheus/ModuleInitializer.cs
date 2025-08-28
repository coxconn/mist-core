using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MistCore.Core.AspNet.Modules;
using System.Collections.Specialized;
using System.Linq;
using System.Collections.Generic;
using System;
using Prometheus;

namespace MistCore.Framework.Prometheus
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

        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            //��������м��
            app.UseMiddleware<HttpHealthMiddleware>("/health");

            //��Ӽ��ģ�� Prometheus
            app.UseMetricServer();

            //��Ӽ��ģ�� Prometheus
            app.UseHttpMetrics(options =>
            {
                //options.RequestCount.Enabled = false;
            });


        }

    }
}

