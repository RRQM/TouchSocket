using System.Text;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace RpcPerformanceConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var consoleAction = new ConsoleAction("h|help|?");//设置帮助命令
            consoleAction.OnException += ConsoleAction_OnException;//订阅执行异常输出

            StartServer();

            var count = 100000;

            consoleAction.Add("3.1", "DmtpRpc测试Sum", () => StartSumClient(count));
            consoleAction.Add("3.2", "DmtpRpc测试GetBytes", () => StartGetBytesClient(count));
            consoleAction.Add("3.3", "DmtpRpc测试BigString", () => StartBigStringClient(count));

            consoleAction.ShowAll();

            await consoleAction.RunCommandLineAsync();
        }

        public static void StartServer()
        {
            var service = new TcpDmtpService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(7789)
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                       a.AddRpcStore(store =>
                       {
                           store.RegisterServer<TestController>();
                       });
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRpc();
                   })
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "Rpc"//设定连接口令，作用类似账号密码
                   });

            service.Setup(config);
            service.Start();

            service.Logger.Info($"{service.GetType().Name}已启动");
        }

        public static void StartSumClient(int count)
        {
            var client = GetClient();
            var timeSpan = TimeMeasurer.Run(() =>
            {
                var actor = client.GetDmtpRpcActor();
                for (var i = 0; i < count; i++)
                {
                    var rs = actor.InvokeT<int>("Sum", InvokeOption.WaitInvoke, i, i);
                    if (rs != i + i)
                    {
                        Console.WriteLine("调用结果不一致");
                    }
                    if (i % 1000 == 0)
                    {
                        Console.WriteLine(i);
                    }
                }
            });
            Console.WriteLine(timeSpan);
        }
        private static TcpDmtpClient GetClient()
        {
            var client = new TcpDmtpClient();
            client.Setup(new TouchSocketConfig()
                //.SetRegistrator(new MyContainer())
                .ConfigurePlugins(a =>
                {
                    a.UseDmtpRpc();
                })
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetDmtpOption(new DmtpOption()
                {
                    VerifyToken = "Rpc"
                }));
            client.Connect();
            return client;
        }
        public static void StartGetBytesClient(int count)
        {
            var client = GetClient();
            var timeSpan = TimeMeasurer.Run(() =>
            {
                var actor = client.GetDmtpRpcActor();
                for (var i = 1; i < count; i++)
                {
                    var rs = actor.InvokeT<byte[]>("GetBytes", InvokeOption.WaitInvoke, i);//测试10k数据
                    if (rs.Length != i)
                    {
                        Console.WriteLine("调用结果不一致");
                    }
                    if (i % 1000 == 0)
                    {
                        Console.WriteLine(i);
                    }
                }
            });
            Console.WriteLine(timeSpan);
        }

        public static void StartBigStringClient(int count)
        {
            var client = GetClient();
            var timeSpan = TimeMeasurer.Run(() =>
            {
                var actor = client.GetDmtpRpcActor();
                for (var i = 0; i < count; i++)
                {
                    var rs = actor.InvokeT<string>("GetBigString", InvokeOption.WaitInvoke);
                    if (i % 1000 == 0)
                    {
                        Console.WriteLine(i);
                    }
                }
            });
            Console.WriteLine(timeSpan);
        }

        private static void ConsoleAction_OnException(Exception ex)
        {
            ConsoleLogger.Default.Exception(ex);
        }
    }

    [AutoInjectForSingleton]
    public partial class TestController : RpcServer
    {
        [DmtpRpc(MethodInvoke =true)]
        public int Sum(int a, int b) => a + b;

        [DmtpRpc(MethodInvoke = true)]
        public byte[] GetBytes(int length)
        {
            return new byte[length];
        }

        [DmtpRpc(MethodInvoke = true)]
        public string GetBigString()
        {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < 10; i++)
            {
                stringBuilder.Append("RRQM");
            }
            return stringBuilder.ToString();
        }
    }

    #region IOC
    /// <summary>
    /// IOC容器
    /// </summary>
    [AddSingletonInject(typeof(IPluginManager), typeof(PluginManager))]
    [AddSingletonInject(typeof(ILog), typeof(LoggerGroup))]
    [AddSingletonInject(typeof(DmtpRpcFeature))]
    [AddSingletonInject(typeof(IRpcServerProvider), typeof(RpcServerProvider))]
    [AddSingletonInject(typeof(IDmtpRouteService), typeof(DmtpRouteService))]
    [GeneratorContainer]
    public partial class MyContainer : ManualContainer
    {
        public override bool IsRegistered(Type fromType, string key = "")
        {
            if (fromType == typeof(IDmtpRouteService))
            {
                return false;
            }
            return base.IsRegistered(fromType, key);
        }
    }
    #endregion
}