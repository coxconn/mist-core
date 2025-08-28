using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MistCore.Core.AspNet.Modules;
using MistCore.Framework.EventBus.RabbitMQ.Config;
using System;
using System.Linq;
using System.Reflection;

namespace MistCore.Framework.EventBus.RabbitMQ
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
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var serivceTypes = assemblies.SelectMany(c => c.GetTypes()).Where(t => !t.GetTypeInfo().IsAbstract && t.GetInterfaces().Any(k => k.IsGenericType && k.GetGenericTypeDefinition() == typeof(IEventHandler<>))).ToList();

            foreach (var serviceType in serivceTypes)
            {
                services.AddScoped(serviceType);
                foreach (var intfType in serviceType.GetInterfaces())
                {
                    services.AddScoped(intfType, serviceType);
                }
            }

            services.AddSingleton<IEventBus>(sp => {

                var configuration = sp.GetService<IConfiguration>();
                var eventBusInfo = new EventBusInfo();

                configuration.GetSection("EventBus").Bind(eventBusInfo);

                var loggerFactory = sp.GetService<ILoggerFactory>();

                var eventHandlerContext = new EventHandlerExecutionContext(sp);

                var eventBus = new RabbitMqEventBus(eventBusInfo, eventHandlerContext, loggerFactory);

                var eventHandlerTypes = assemblies.SelectMany(c => c.GetTypes()).Where(t => !t.GetTypeInfo().IsAbstract && t.GetInterfaces().Any(k => k.IsGenericType && k.GetGenericTypeDefinition() == typeof(IEventHandler<>))).ToList();
                foreach (var eventHandlerType in eventHandlerTypes)
                {
                    var routeKey = eventHandlerType.GetCustomAttribute<SubscribeAttribute>()?.RouteKey;
                    if (!string.IsNullOrEmpty(routeKey))
                    {
                        Type eventType = eventHandlerType.GetInterfaces()[0].GetGenericArguments()[0];

                        MethodInfo mi1 = eventBus.GetType().GetMethod("Register");
                        MethodInfo mi2 = mi1.MakeGenericMethod(eventType, eventHandlerType);
                        mi2.Invoke(eventBus, new object[] { routeKey });
                    }
                }

                return eventBus;
            });

        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            var eventBus = app.ApplicationServices.GetService<IEventBus>();
        }

    }
}

