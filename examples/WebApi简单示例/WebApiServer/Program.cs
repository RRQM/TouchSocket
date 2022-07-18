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
            HttpService service = new HttpService();
            service.Setup(new TouchSocketConfig()
               .UsePlugin()
               .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
               .ConfigureRpcStore(a =>
               {
                   a.RegisterServer<Server>();//注册服务
               }))
               .Start();

            var webApiParser = service.AddPlugin<WebApiParserPlugin>();

            Console.WriteLine("以下连接用于测试webApi");
            Console.WriteLine($"使用：http://127.0.0.1:7789/Server/Sum?a=10&b=20");

            //RpcStore.ProxyAttributeMap.TryAdd("webapi", typeof(WebApiAttribute));
            //生成的WebApi的本地代理文件。
            //ServerCellCode[] cellCodes = rpcStore.GetProxyInfo(RpcStore.ProxyAttributeMap.Values.ToArray());
            //当想导出全部时，RpcStore.ProxyAttributeMap.Values.ToArray()
            //string codeString = CodeGenerator.ConvertToCode("RRQMProxy", cellCodes);
            Console.ReadKey();
        }
    }

    public class Server : RpcServer
    {
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
        public Task<string> DownloadFile(WebApiServerCallContext callContext, string id)
        {
            if (id == "rrqm")
            {
                callContext.Context.Response.FromFile(@"D:\System\Windows.iso", callContext.Context.Request);
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