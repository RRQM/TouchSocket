using FastEndpoints;
using HttpPerformanceConsoleApp.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.WebApi.Swagger;
using static System.Net.WebRequestMethods;

namespace HttpPerformanceConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            StartAspnetHttp();
            StartTouchSokcetHttp();
            StartFastEndpoints();
            Console.ReadKey();
        }

        static void StartTouchSokcetHttp()
        {
            var service = new HttpService();
            service.Setup(new TouchSocketConfig()
               .SetListenIPHosts(7790)
               .ConfigureContainer(a => 
               {
                   a.AddRpcStore(store =>
                   {
                       store.RegisterServer<ApiServer>();//注册服务
                   });
               })
               .ConfigurePlugins(a =>
               {
                   a.UseWebApi();

                   a.UseSwagger()//使用Swagger页面
                   .UseLaunchBrowser();//启动浏览器

                   //此插件是http的兜底插件，应该最后添加。作用是当所有路由不匹配时返回404.且内部也会处理Option请求。可以更好的处理来自浏览器的跨域探测。
                   a.UseDefaultHttpServicePlugin();
               }));
            service.Start();

            ConsoleLogger.Default.Info($"TouchSokcetHttp已启动，请求连接：http://127.0.0.1:7790/ApiServer/Add?a=10&b=20");
        }

        static void StartAspnetHttp()
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

        static void StartFastEndpoints()
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