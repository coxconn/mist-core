using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MistCore.Framework.Prometheus
{
    public class HttpHealthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly string _healthUrlPath;

        /// <summary>
        /// 计时器
        /// </summary>
        private Stopwatch _stopwatch;

        /// <summary>
        /// 构造 Http 请求中间件
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="cacheService"></param>
        public HttpHealthMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, string healthUrlPath)
        {
            _healthUrlPath = healthUrlPath ?? "/api/health";
            _next = next;
            _logger = loggerFactory.CreateLogger<HttpHealthMiddleware>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            if(context.Request.Path == _healthUrlPath)
            {
                context.Request.EnableBuffering();
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
                _logger.LogDebug($"Handling request: " + context.Request.Path);
                //var api = new ApiRequestInputViewModel
                //{
                //    HttpType = context.Request.Method,
                //    Query = context.Request.QueryString.Value,
                //    RequestUrl = context.Request.Path,
                //    RequestName = "",
                //    RequestIP = context.Request.Host.Value
                //};

                //var request = context.Request.Body;
                var response = context.Response;

                var result = new { result = "ok" };
                //context.Response.Body.Write(Encoding.UTF8.GetBytes(result.result));
                //context.Response.Body.Position = 0;
                response.StatusCode = 200;
                await response.WriteAsync(result.result);

                // 响应完成时存入缓存
                context.Response.OnCompleted(() =>
                {
                    var elapsedTime = _stopwatch.ElapsedMilliseconds;
                    _stopwatch.Stop();

                    _logger.LogDebug($"RequestLog:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss-fff")} {elapsedTime} ms");
                    return Task.CompletedTask;
                });

                _logger.LogDebug($"Finished handling request.{_stopwatch.ElapsedMilliseconds}ms");
            }
            else
            {
                await _next(context);
            }

        }


    }
}
