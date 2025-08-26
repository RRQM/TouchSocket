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

using System.Text.Json.Serialization;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.WebApi;

namespace WebApiConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.ConfigureContainer(a =>
            {
                a.AddRpcStore(store =>
                {
                    store.RegisterServer<ApiServer>();//注册服务
                });

                a.AddAspNetCoreLogger();
            });

            builder.Services.AddServiceHostedService<IHttpService, HttpService>(config =>
            {
                config.SetListenIPHosts(7789)
                .ConfigurePlugins(a =>
                {
                    a.UseTcpSessionCheckClear();

                    a.UseWebApi()
                    .ConfigureConverter(converter =>
                    {
                        converter.Clear();
                        converter.AddSystemTextJsonSerializerFormatter(options =>
                        {
                            options.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
                        });
                    });

                    //a.UseSwagger()
                    //.UseLaunchBrowser();

                    //a.UseHttpStaticPage()
                    //.SetNavigateAction(request =>
                    //{
                    //    //此处可以设置重定向
                    //    return request.RelativeURL;
                    //})
                    //.SetResponseAction(response =>
                    //{
                    //    //可以设置响应头
                    //})
                    //.AddFolder("api/");//添加静态页面文件夹，可使用 http://127.0.0.1:7789/index.html 访问静态网页

                    ////此插件是http的兜底插件，应该最后添加。作用是当所有路由不匹配时返回404.且内部也会处理Option请求。可以更好的处理来自浏览器的跨域探测。
                    //a.UseDefaultHttpServicePlugin();
                });
            });

            var host = builder.Build();
            host.Run();
        }
    }


    [JsonSerializable(typeof(MyClass))]
    [JsonSerializable(typeof(MySum))]
    internal partial class AppJsonSerializerContext : JsonSerializerContext
    {

    }

    public partial class ApiServer : SingletonRpcServer
    {
        private readonly ILog m_logger;

        public ApiServer(ILog logger)
        {
            this.m_logger = logger;
        }

        [Router("[api]/[action]ab")]//此路由会以"/Server/Sumab"实现
        [Router("[api]/[action]")]//此路由会以"/Server/Sum"实现
        [WebApi(Method = HttpMethodType.Get)]
        public int Sum(int a, int b)
        {
            //m_logger.Info("Sum");
            return a + b;
        }

        [WebApi(Method = HttpMethodType.Post)]
        public MySum TestPost(MyClass myClass)
        {
            this.m_logger.Info("TestPost");
            return new MySum() { A = myClass.A, B = myClass.B, Sum = myClass.A + myClass.B };
        }

    }

    public class MyClass
    {
        public int A { get; set; }
        public int B { get; set; }
    }

    public class MySum : MyClass
    {
        public int Sum { get; set; }
    }
}