using System;
using TouchSocket.Core;
using TouchSocket.Rpc;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace ReverseRpcConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //创建逻辑服务器
            TcpTouchRpcService tcpTouchRpcService = CreateTcpTouchRpcService(7789);

            //创建逻辑客户端
            TcpTouchRpcClient client = CreateTcpTouchRpcClient();

            foreach (var item in tcpTouchRpcService.GetClients())
            {
                client.Logger.Info(item.Invoke<string>("ReverseRpcConsoleApp.ReverseCallbackServer.SayHello".ToLower(), InvokeOption.WaitInvoke, "张三"));
            }

            Console.ReadKey();
        }

        private static TcpTouchRpcService CreateTcpTouchRpcService(int port)
        {
            var service = new TcpTouchRpcService();
            TouchSocketConfig config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(port) })
                   .ConfigureContainer(a =>
                   {
                       a.SetLogger<LoggerGroup<ConsoleLogger, FileLogger>>();//注册一个日志组
                   })
                   .SetVerifyToken("TouchRpc");

            service.Setup(config)
                .Start();

            service.Logger.Info($"{service.GetType().Name}已启动，监听端口：{port}");
            return service;
        }

        private static TcpTouchRpcClient CreateTcpTouchRpcClient()
        {
            TcpTouchRpcClient client = new TcpTouchRpcClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .ConfigureContainer(a =>
                {
                    a.SetLogger<LoggerGroup<ConsoleLogger, FileLogger>>();//注册一个日志组
                })
                .ConfigureRpcStore(a =>
                {
                    a.RegisterServer<ReverseCallbackServer>();
                })
                .SetVerifyToken("TouchRpc"));
            client.Connect();

            return client;
        }
    }

    public class ReverseCallbackServer : RpcServer
    {
        [TouchRpc]
        public string SayHello(string name)
        {
            return $"{name},hi";
        }
    }
}