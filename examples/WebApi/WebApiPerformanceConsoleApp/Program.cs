// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using FastEndpoints;
using HttpPerformanceConsoleApp.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace HttpPerformanceConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            StartAspnetHttp();
            StartTouchSokcetHttp();
            StartFastEndpoints();
            Console.ReadKey();
        }

        private static void StartTouchSokcetHttp()
        {
            var host = Host.CreateDefaultBuilder()
        .ConfigureServices(services =>
        {
            services.AddServiceHostedService<IHttpService, HttpService>(config =>
            {
                config.SetListenIPHosts(7790)
               .ConfigureContainer(a =>
               {
                   a.AddRpcStore(store =>
                   {
                       store.RegisterServer<ApiServer>();//注册服务
                   });

                   a.AddConsoleLogger();
               })
               .ConfigurePlugins(a =>
               {
                   a.UseWebApi();

                   //a.UseSwagger()//使用Swagger页面
                   //.UseLaunchBrowser();//启动浏览器

                   //此插件是http的兜底插件，应该最后添加。作用是当所有路由不匹配时返回404.且内部也会处理Option请求。可以更好的处理来自浏览器的跨域探测。
                   a.UseDefaultHttpServicePlugin();
               });
            });
        })
        .Build();

            host.RunAsync();
            ConsoleLogger.Default.Info($"TouchSokcetHttp已启动，请求连接：http://127.0.0.1:7790/ApiServer/Add?a=10&b=20");
        }

        private static void StartAspnetHttp()
        {
            var builder = WebApplication.CreateBuilder();

            builder.Logging.ClearProviders();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            var app = builder.Build();
            app.MapControllers();

            app.RunAsync("http://127.0.0.1:7789");
            ConsoleLogger.Default.Info("Aspnet已启动，请求连接：http://127.0.0.1:7789/ApiServer/Add?a=10&b=20");

        }

        private static void StartFastEndpoints()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Logging.ClearProviders();
            builder.Services.AddFastEndpoints();

            var app = builder.Build();
            app.UseFastEndpoints();
            app.RunAsync("http://127.0.0.1:7791");
            ConsoleLogger.Default.Info("FastEndpoints已启动，请求连接：http://127.0.0.1:7791/ApiServer/Add?a=10&b=20");
        }
    }
}