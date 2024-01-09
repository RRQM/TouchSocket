using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.WebApi;

namespace WebApiConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var service = new HttpService();
            service.Setup(new TouchSocketConfig()
               .SetListenIPHosts(7789)
               //.SetRegistrator(new MyContainer())
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
               }));
            service.Start();

            Console.WriteLine("以下连接用于测试webApi");
            Console.WriteLine($"使用：http://127.0.0.1:7789/ApiServer/Sum?a=10&b=20");
            Console.ReadKey();
        }
    }

    #region IOC
    /// <summary>
    /// IOC容器
    /// </summary>
    [AddSingletonInject(typeof(IPluginManager), typeof(PluginManager))]
    [AddSingletonInject(typeof(ILog), typeof(LoggerGroup))]
    [AddSingletonInject(typeof(IRpcServerProvider), typeof(RpcServerProvider))]
    [AddSingletonInject(typeof(ApiServer))]
    [AddSingletonInject(typeof(WebApiParserPlugin))]
    [AddSingletonInject(typeof(DefaultHttpServicePlugin))]
    [GeneratorContainer]
    public partial class MyContainer : ManualContainer
    {
    }
    #endregion

    public partial class ApiServer : RpcServer
    {
        private readonly ILog m_logger;

        public ApiServer(ILog logger)
        {
            this.m_logger = logger;
        }

        [Origin(AllowOrigin = "*")]//跨域设置
        [Router("[api]/[action]ab")]//此路由会以"/Server/Sumab"实现
        [Router("[api]/[action]")]//此路由会以"/Server/Sum"实现
        [WebApi(HttpMethodType.GET)]
        public int Sum(int a, int b)
        {
            return a + b;
        }

       
    }
}
