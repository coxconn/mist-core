using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using MistCore.Core.AspNet.Modules;
using MistCore.Core.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IApplicationBuilderExtensions
    {
        public static void UseCustomizedConfigure(this IApplicationBuilder app, IHostEnvironment env)
        {

            var moduleInitializers = app.ApplicationServices.GetServices<IModuleInitializer>();
            foreach (var moduleInitializer in moduleInitializers)
            {
                if (typeof(IModuleAspNetInitializer).IsAssignableFrom(moduleInitializer.GetType()))
                {
                    (moduleInitializer as IModuleAspNetInitializer).Configure(app, env);
                }
            }

            //GlobalConfiguration.ServiceLocator = new ServiceScopedLocator(app.ApplicationServices);
            app.ApplicationServices.ServiceScopedLocator();

        }
    }
}
