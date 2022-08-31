using System;
using System.Threading.Tasks;
using TouchSocket.Core.Config;
using TouchSocket.Core.Plugins;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Rpc.WebApi;
using TouchSocket.Sockets;

namespace WebApiServerApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            WebApiParserPlugin webApiParser=null;
            HttpService service = new HttpService();
            service.Setup(new TouchSocketConfig()
               .UsePlugin()
               .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
               .ConfigureRpcStore(a =>
               {
                   a.RegisterServer<Server>();//注册服务
               })
               .ConfigurePlugins(a=> 
               {
                   webApiParser= a.Add<WebApiParserPlugin>();
               }))
               .Start();

            Console.WriteLine("以下连接用于测试webApi");
            Console.WriteLine($"使用：http://127.0.0.1:7789/Server/Sum?a=10&b=20");

            //下列代码，会生成客户端的调用代码。
            string codeString = webApiParser.RpcStore.GetProxyCodes("WebApiProxy");
            Console.ReadKey();
        }
    }

    public class Server : RpcServer
    {
        [Router("[api]/[action]ab")]//此路由会以"/Server/Sumab"实现
        [Router("[api]/[action]")]//此路由会以"/Server/Sum"实现
        [WebApi(HttpMethodType.GET)]
        public int Sum(int a, int b)
        {
            return a + b;
        }

        [WebApi(HttpMethodType.POST)]
        public int TestPost(MyClass myClass)
        {
            return myClass.A + myClass.B;
        }

        /// <summary>
        /// 使用调用上下文，响应文件下载。
        /// </summary>
        /// <param name="callContext"></param>
        [WebApi(HttpMethodType.GET, MethodFlags = MethodFlags.IncludeCallContext)]
        public Task<string> DownloadFile(IWebApiCallContext callContext, string id)
        {
            if (id == "rrqm")
            {
                callContext.HttpContext.Response.FromFile(@"D:\System\Windows.iso", callContext.HttpContext.Request);
                return Task.FromResult("ok");
            }
            return Task.FromResult("id不正确。");
        }
    }

    public class MyClass
    {
        public int A { get; set; }
        public int B { get; set; }
    }
}