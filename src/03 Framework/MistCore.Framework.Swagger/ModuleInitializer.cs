using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MistCore.Core.AspNet.Modules;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using MistCore.Core.Modules;
using IGeekFan.AspNetCore.Knife4jUI;
using MistCore.Framework.Swagger.Config;

namespace MistCore.Framework.Swagger
{
    /// <summary>
    /// ModuleInitializer
    /// </summary>
    public class ModuleInitializer : IModuleAspNetInitializer
    {
        private Dictionary<string, string> swaggerDic = new Dictionary<string, string>();
        private List<string> filePaths = new List<string>();
        private SwaggerConfig swaggerConfig = new SwaggerConfig();


        public void ConfigureServices(IServiceCollection services)
        {

            GlobalConfiguration.Configuration.GetSection("SwaggerConfig").Bind(swaggerConfig);

            #region 获取所有的生成xml文件

            //获取所有程序集的生成xml文件
            var files = AppDomain.CurrentDomain.GetAssemblies()
                .Select(c => new
                {
                    Assembly = c,
                    Name = c.GetName().Name,
                    File = new FileInfo(Path.Combine(AppContext.BaseDirectory, c.GetName().Name + ".xml"))
                })
                .Where(c => c.File.Exists)
                .ToList();

            //需要加载的xml 文件路径
            filePaths = files.Select(c => c.File.FullName).ToList();

            //所有api 的Group Name
            var groupNames = files.SelectMany(c => c.Assembly.GetTypes())
                .SelectMany(t => t.GetCustomAttributes(typeof(ApiExplorerSettingsAttribute), true).OfType<ApiExplorerSettingsAttribute>())
                .Select(c => c.GroupName)
                .Distinct();

            //所有的Group 字典
            swaggerDic = groupNames.Select(c => new
            {
                Key = c,
                Value = swaggerConfig.HostName,
            }).ToDictionary(c => c.Key, c => c.Value);

            if (swaggerDic.Count == 0)
            {
                swaggerDic.Add("v1", swaggerConfig.HostName);
            }
            #endregion

            services.AddSingleton(typeof(SwaggerConfig), sp =>
            {
                var configuration = sp.GetService<IConfiguration>();

                var config = new SwaggerConfig();
                configuration.GetSection("SwaggerConfig").Bind(config);

                return config;
            });

            services.AddSwaggerGen(s =>
            {
                foreach (KeyValuePair<string, string> kv in swaggerDic)
                {
                    s.SwaggerDoc(kv.Key, new OpenApiInfo
                    {
                        Version = kv.Key,
                        Title = $"{kv.Value} API",
                        //Description = $" {kv.Value} api 文档"
                    });
                }

                filePaths.ForEach(path =>
                {
                    s.IncludeXmlComments(path, true);
                });

                //s.EnableAnnotations();
                //s.DocInclusionPredicate((docName, description) => true);

                if ("Bearer".Equals(swaggerConfig.Security, StringComparison.OrdinalIgnoreCase))
                {
                    s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT授权(数据将在请求头中进行传输) 直接在下框中输入{token}\"",
                        Name = "Authorization",//jwt默认的参数名称
                        In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
                        Type = SecuritySchemeType.ApiKey
                    });

                    s.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                    {new OpenApiSecurityScheme
                    {
                        Reference=new OpenApiReference()
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    }, Array.Empty<string>() }
                    });
                }

            });
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {

            if (!string.IsNullOrWhiteSpace(swaggerConfig.HostName))
            {
                swaggerConfig.Prefix = (swaggerConfig.Prefix ?? string.Empty).TrimStart('/').TrimEnd('/');

                if (!string.IsNullOrWhiteSpace(swaggerConfig.Prefix))
                {
                    app.UseSwagger(c =>
                    {
                        c.RouteTemplate = $"{swaggerConfig.Prefix}/knife/{{documentName}}/swagger.json";
                    });
                    app.UseSwaggerUI(c =>
                    {
                        foreach (KeyValuePair<string, string> kv in swaggerDic)
                        {
                            c.SwaggerEndpoint($"/{swaggerConfig.Prefix}/knife/{kv.Key}/swagger.json", kv.Value + " " + kv.Key);
                            c.RoutePrefix = $"{swaggerConfig.Prefix}/swagger";
                        }
                    });
                    app.UseKnife4UI(c =>
                    {
                        foreach (KeyValuePair<string, string> kv in swaggerDic)
                        {
                            c.SwaggerEndpoint($"/{kv.Key}/swagger.json", kv.Value + " " + kv.Key);
                            c.RoutePrefix = $"{swaggerConfig.Prefix}/knife";
                        }
                    });
                }
                else
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        foreach (KeyValuePair<string, string> kv in swaggerDic)
                        {
                            c.SwaggerEndpoint($"/swagger/{kv.Key}/swagger.json", kv.Value + " " + kv.Key);
                            c.RoutePrefix = "swagger";
                        }
                    });
                }
            }

        }

    }
}

