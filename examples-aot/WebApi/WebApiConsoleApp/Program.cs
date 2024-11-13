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

            builder.Services.AddServiceHostedService<IHttpService, HttpService>(config =>
            {
                config.SetListenIPHosts(7789)
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
                    //a.UseCheckClear();

                    a.UseWebApi();

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

    public partial class ApiServer : RpcServer
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
        public int TestPost(MyClass myClass)
        {
            m_logger.Info("TestPost");
            return myClass.A + myClass.B;
        }

    }

    public class MyClass
    {
        public int A { get; set; }
        public int B { get; set; }
    }
}