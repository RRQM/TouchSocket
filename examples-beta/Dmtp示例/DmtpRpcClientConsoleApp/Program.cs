using System.Text;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ClientConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {

            //Run_1();
            Run_2();

            Console.ReadKey();
        }

        /// <summary>
        /// 客户端互相调用Rpc
        /// </summary>
        static void Run_2()
        {
            var client1 = GetTcpDmtpClient();
            var client2 = GetTcpDmtpClient();

            client1.GetDmtpRpcActor().InvokeT<bool>(client2.Id, "Notice", InvokeOption.WaitInvoke,"Hello");

            //使用下面方法targetRpcClient也能使用代理调用。
            var targetRpcClient = client1.CreateTargetDmtpRpcActor(client2.Id);
            targetRpcClient.InvokeT<bool>("Notice", InvokeOption.WaitInvoke, "Hello");
        }

        /// <summary>
        /// 直接调用Rpc
        /// </summary>
        static void Run_1()
        {
            var client = GetTcpDmtpClient();
            int sum = client.GetDmtpRpcActor().InvokeT<int>("Add", DmtpInvokeOption.WaitInvoke, 10, 20);
            client.Logger.Info($"调用Add方法成功，结果：{sum}");
        }

        static TcpDmtpClient GetTcpDmtpClient()
        {
            var client = new TcpDmtpClient();
            client.Setup(new TouchSocketConfig()
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.UseDmtpRpc()
                    .ConfigureRpcStore(store =>
                    {
                        store.RegisterServer<MyClientRpcServer>();
                    });
                })
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetVerifyToken("Dmtp"));
            client.Connect();
            client.Logger.Info($"连接成功，Id={client.Id}");
            return client;
        }

        class MyClientRpcServer : RpcServer
        {
            private readonly ILog m_logger;

            public MyClientRpcServer(ILog logger)
            {
                this.m_logger = logger;
            }

            [DmtpRpc(true)]//使用函数名直接调用
            public bool Notice(string msg)
            {
                this.m_logger.Info(msg);
                return true;
            }
        }
    }
}