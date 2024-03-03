using TouchSocket.Core;
using TouchSocket.Sockets;
using TouchSocket.WebApi;

namespace DispatchProxyWebApiConsoleApp
{
    internal class Program
    {
        /// <summary>
        /// 使用DispatchProxy生成调用代理
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            var api = MyWebApiDispatchProxy.Create<IApiServer, MyWebApiDispatchProxy>();
            while (true)
            {
                Console.WriteLine("请输入两个数，中间用空格隔开，回车确认");
                var str = Console.ReadLine();
                var strs = str.Split(' ');
                var a = int.Parse(strs[0]);
                var b = int.Parse(strs[1]);

                var sum = api.Sum(a, b);
                Console.WriteLine(sum);
            }
        }
    }

    /// <summary>
    /// 新建一个类，继承WebApiDispatchProxy，亦或者RpcDispatchProxy基类。
    /// 然后实现抽象方法，主要是能获取到调用的IRpcClient派生接口。
    /// </summary>
    internal class MyWebApiDispatchProxy : WebApiDispatchProxy
    {
        private readonly WebApiClient m_client;

        public MyWebApiDispatchProxy()
        {
            this.m_client = CreateWebApiClient();
        }

        private static WebApiClient CreateWebApiClient()
        {
            var client = new WebApiClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .ConfigurePlugins(a =>
                {
                    a.UseReconnection();
                }));
            client.Connect();
            Console.WriteLine("连接成功");
            return client;
        }

        public override IWebApiClientBase GetClient()
        {
            return this.m_client;
        }
    }

    internal interface IApiServer
    {
        [Router("ApiServer/[action]ab")]
        [Router("ApiServer/[action]")]
        [WebApi(HttpMethodType.GET)]
        int Sum(int a, int b);
    }
}