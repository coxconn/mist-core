using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MistCore.Core.AspNet.Modules;
using System.Collections.Specialized;
using System.Linq;
using System.Collections.Generic;
using System;
using SkyApm.AspNetCore.Diagnostics;

namespace MistCore.Framework.SkyAPM
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
            //APM
            services.AddSkyAPM(ext => ext.AddAspNetCoreHosting());
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {

        }

    }
}

