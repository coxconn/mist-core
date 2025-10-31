using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MistCore.Core.AspNet.Modules;
using MistCore.Core.Modules;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Reflection;

namespace MistCore.Framework.Cached.RedisProvider
{
    /// <summary>
    /// ModuleInitializer
    /// </summary>
    public class ModuleInitializer : IModuleAspNetInitializer
    {
        public void ConfigureServices(IServiceCollection services)
        { 
            var configString = GlobalConfiguration.Configuration.GetSection("RedisConnectionStrings").Value;
            ConfigurationOptions configOptions = ConfigurationOptions.Parse(configString);
            configOptions.ReconnectRetryPolicy = new ExponentialRetry(5000, 10000);

            #region StackExchange.Redis IDistributedCache

            services.AddStackExchangeRedisCache(options =>
            {
                //options.InstanceName = GlobalConfiguration.Configuration.GetSection("RedisConnections")["InstanceName"];
                options.ConfigurationOptions = configOptions;
            });

            services.AddTransient(typeof(ICache), typeof(RedisCache));
            services.AddTransient(typeof(RedisCache));
            #endregion

            #region StackExchange.Redis IDatabase
            //var configurationOptions = new ConfigurationOptions()
            //{
            //    Password = GlobalConfiguration.Configuration.GetSection("RedisConnections")["Password"],
            //    DefaultDatabase = Convert.ToInt32(GlobalConfiguration.Configuration.GetSection("RedisConnections")["DataBase"]),
            //    ConnectTimeout = 5000,//设置建立连接到Redis服务器的超时时间为5000毫秒
            //    SyncTimeout = 5000,//设置对Redis服务器进行同步操作的超时时间为5000毫秒
            //    Ssl = false,//设置Redis启用SSL安全加密传输数据。
            //    SslProtocols = System.Security.Authentication.SslProtocols.Tls,
            //};
            //configurationOptions.EndPoints.Add(GlobalConfiguration.Configuration.GetSection("RedisConnections")["IP"]);
            //var connectionMultiplexer = ConnectionMultiplexer.Connect(configurationOptions);

            var connectionMultiplexer = ConnectionMultiplexer.Connect(configOptions);
            //connectionMultiplexer.PreserveAsyncOrder = false; //无序执行
            services.AddSingleton<IConnectionMultiplexer>(service => connectionMultiplexer);
            services.AddSingleton<IDatabase>(service => connectionMultiplexer.GetDatabase());
            services.AddSingleton(typeof(RedisClient));
            #endregion
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {

        }

    }
}

