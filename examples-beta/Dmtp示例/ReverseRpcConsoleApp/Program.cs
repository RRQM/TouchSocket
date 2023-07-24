using System;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ReverseRpcConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //创建逻辑服务器
            var tcpDmtpService = CreateTcpDmtpService(7789);

            //创建逻辑客户端
            var client = CreateTcpDmtpClient();

            foreach (var item in tcpDmtpService.GetClients())
            {
                client.Logger.Info(item.GetDmtpRpcActor().InvokeT<string>("SayHello", InvokeOption.WaitInvoke, "张三"));
                client.Logger.Info("调用完成");
            }

            Console.ReadKey();
        }

        private static TcpDmtpService CreateTcpDmtpService(int port)
        {
            var service = new TcpDmtpService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(port) })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRpc();
                   })
                   .SetVerifyToken("Dmtp");

            service.Setup(config)
                .Start();

            service.Logger.Info($"{service.GetType().Name}已启动，监听端口：{port}");
            return service;
        }

        private static TcpDmtpClient CreateTcpDmtpClient()
        {
            var client = new TcpDmtpClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                 .ConfigurePlugins(a =>
                 {
                     a.UseDmtpRpc()
                     .ConfigureRpcStore(store =>
                     {
                         store.RegisterServer<ReverseCallbackServer>();
                     });
                 })
                .SetVerifyToken("Dmtp"));
            client.Connect();

            return client;
        }
    }

    public class ReverseCallbackServer : RpcServer
    {
        [DmtpRpc(MethodInvoke = true)]
        public string SayHello(string name)
        {
            return $"{name},hi";
        }
    }
}