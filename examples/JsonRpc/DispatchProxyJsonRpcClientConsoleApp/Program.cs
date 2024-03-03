using TouchSocket.Core;
using TouchSocket.JsonRpc;
using TouchSocket.Sockets;

namespace DispatchProxyJsonRpcClientConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var rpc = MyJsonRpcDispatchProxy.Create<IJsonRpcServer, MyJsonRpcDispatchProxy>();

            while (true)
            {
                var result = rpc.TestJsonRpc(Console.ReadLine());
                Console.WriteLine(result);
            }
        }
    }

    /// <summary>
    /// 新建一个类，继承JsonRpcDispatchProxy，亦或者RpcDispatchProxy基类。
    /// 然后实现抽象方法，主要是能获取到调用的IRpcClient派生接口。
    /// </summary>
    internal class MyJsonRpcDispatchProxy : JsonRpcDispatchProxy
    {
        private readonly IJsonRpcClient m_client;

        public MyJsonRpcDispatchProxy()
        {
            this.m_client = CreateJsonRpcClientByTcp();
        }

        public override IJsonRpcClient GetClient()
        {
            return this.m_client;
        }

        private static IJsonRpcClient CreateJsonRpcClientByTcp()
        {
            var client = new TcpJsonRpcClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7705")
                .SetTcpDataHandlingAdapter(() => new TerminatorPackageAdapter("\r\n")));
            client.Connect();

            Console.WriteLine("连接成功");
            return client;
        }
    }

    internal interface IJsonRpcServer
    {
        [JsonRpc(MethodInvoke = true)]
        string TestJsonRpc(string str);
    }
}