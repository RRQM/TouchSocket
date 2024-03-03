using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace HttpServiceForCorsConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = new HttpService();
            service.Setup(new TouchSocketConfig()//加载配置
                .SetListenIPHosts(7789)
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();

                    //添加跨域服务
                    a.AddCors(corsOption =>
                    {
                        //添加跨域策略，后续使用policyName即可应用跨域策略。
                        corsOption.Add("cors", corsBuilder =>
                        {
                            corsBuilder.AllowAnyMethod()
                                .AllowAnyOrigin();
                        });
                    });
                })
                .ConfigurePlugins(a =>
                {
                    //应用名称为cors的跨域策略。
                    a.UseCors("cors");

                    //default插件应该最后添加，其作用是
                    //1、为找不到的路由返回404
                    //2、处理header为Option的探视跨域请求。
                    a.UseDefaultHttpServicePlugin();
                }));
            service.Start();

            Console.WriteLine("Http服务器已启动");
            Console.ReadKey();
        }
    }
}