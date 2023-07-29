using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ConsoleApp2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var service = new TcpDmtpService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(7789)
                   .ConfigureContainer(a =>
                   {
                       a.AddDmtpRouteService();
                       a.AddConsoleLogger();
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRpc()
                       .ConfigureRpcStore(store =>
                       {
                           store.RegisterServer<MyRpcServer>();
                       });

                       a.Add<MyTouchRpcPlugin>();
                   })
                   .SetVerifyToken("Dmtp");//设定连接口令，作用类似账号密码

            service.Setup(config)
                .Start();


            service.Logger.Info($"{service.GetType().Name}已启动");

            service.Logger.Info($"输入客户端Id，空格输入消息，将通知客户端方法");
            while (true)
            {
                string str = Console.ReadLine();
                if (service.TryGetSocketClient(str.Split(' ')[0], out var socketClient))
                {
                    bool result = socketClient.GetDmtpRpcActor().InvokeT<bool>("Notice", DmtpInvokeOption.WaitInvoke, str.Split(' ')[1]);

                    service.Logger.Info($"调用结果{result}");
                }
            }

        }

        class MyRpcServer : RpcServer
        {
            private readonly ILog m_logger;

            public MyRpcServer(ILog logger)
            {
                this.m_logger = logger;
            }
            [DmtpRpc(true)]//使用函数名直接调用
            public int Add(int a, int b)
            {
                this.m_logger.Info("调用Add");
                int sum = a + b;
                return sum;
            }
        }

        internal class MyTouchRpcPlugin : PluginBase,IDmtpRoutingPlugin
        {
            async Task IDmtpRoutingPlugin<IDmtpActorObject>.OnDmtpRouting(IDmtpActorObject client, PackageRouterEventArgs e)
            {
                if (e.RouterType == RouteType.Rpc)
                {
                    e.IsPermitOperation = true;
                    return;
                }

               await e.InvokeNext();
            }
        }
    }
}