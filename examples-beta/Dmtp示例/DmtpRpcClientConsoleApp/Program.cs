using RpcProxy;
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

            ConsoleAction consoleAction = new ConsoleAction();
            consoleAction.OnException += ConsoleAction_OnException;

            consoleAction.Add("1", "直接调用Rpc", RunInvokeT);
            consoleAction.Add("2", "客户端互相调用Rpc", RunInvokeT_2C);
            consoleAction.Add("3", "测试客户端请求，服务器响应大量流数据", () => { RunRpcPullChannel(); });

            consoleAction.ShowAll();

            while (true)
            {
                if (!consoleAction.Run(Console.ReadLine()))
                {
                    consoleAction.ShowAll();
                }
            }
        }

        private static void ConsoleAction_OnException(Exception obj)
        {
            ConsoleLogger.Default.Exception(obj);
        }

        /// <summary>
        /// 客户端互相调用Rpc
        /// </summary>
        static void RunInvokeT_2C()
        {
            var client1 = GetTcpDmtpClient();
            var client2 = GetTcpDmtpClient();

            client1.GetDmtpRpcActor().InvokeT<bool>(client2.Id, "Notice", InvokeOption.WaitInvoke, "Hello");

            //使用下面方法targetRpcClient也能使用代理调用。
            var targetRpcClient = client1.CreateTargetDmtpRpcActor(client2.Id);
            targetRpcClient.InvokeT<bool>("Notice", InvokeOption.WaitInvoke, "Hello");
        }

        /// <summary>
        /// 直接调用Rpc
        /// </summary>
        static void RunInvokeT()
        {
            var client = GetTcpDmtpClient();
            int sum = client.GetDmtpRpcActor().InvokeT<int>("Add", DmtpInvokeOption.WaitInvoke, 10, 20);
            client.Logger.Info($"调用Add方法成功，结果：{sum}");
        }

        static async void RunRpcPullChannel()
        {
            var client = GetTcpDmtpClient();
            ChannelStatus status = ChannelStatus.Default;
            int size = 0;
            var channel = client.CreateChannel();//创建通道
            Task task = Task.Run(() =>//这里必须用异步
            {
                using (channel)
                {
                    foreach (var currentByteBlock in channel)
                    {
                        size += currentByteBlock.Len;//此处可以处理传递来的流数据
                    }
                    status = channel.Status;//最后状态
                }
            });
            int result = client.GetDmtpRpcActor().RpcPullChannel(channel.Id);//RpcPullChannel是代理方法，此处会阻塞至服务器全部发送完成。
            await task;//等待异步接收完成
            Console.WriteLine($"状态：{status}，size={size}");
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