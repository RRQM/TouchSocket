using System.Text;
using System.Threading.Tasks;
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

            await StartServer();

            var count = 100000;

            consoleAction.Add("3.1", "DmtpRpc测试Sum", async () =>await StartSumClient(count));
            consoleAction.Add("3.2", "DmtpRpc测试GetBytes", async () => await StartGetBytesClient(count));
            consoleAction.Add("3.3", "DmtpRpc测试BigString", async () =>await StartBigStringClient(count));

            consoleAction.ShowAll();

            await consoleAction.RunCommandLineAsync();
        }

        public static async Task StartServer()
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

            await service.SetupAsync(config);
            await service.StartAsync();

            service.Logger.Info($"{service.GetType().Name}已启动");
        }

        public static async Task StartSumClient(int count)
        {
            var client =await GetClient();
            var timeSpan = TimeMeasurer.Run(async () =>
            {
                var actor = client.GetDmtpRpcActor();
                for (var i = 0; i < count; i++)
                {
                    var rs =await actor.InvokeTAsync<int>("Sum", InvokeOption.WaitInvoke, i, i);
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
        private static async Task<TcpDmtpClient> GetClient()
        {
            var client = new TcpDmtpClient();
            await client.SetupAsync(new TouchSocketConfig()
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
            await client.ConnectAsync();
            return client;
        }
        public static async Task StartGetBytesClient(int count)
        {
            var client = await GetClient();
            var timeSpan = TimeMeasurer.Run(async () =>
            {
                var actor = client.GetDmtpRpcActor();
                for (var i = 1; i < count; i++)
                {
                    var rs =await actor.InvokeTAsync<byte[]>("GetBytes", InvokeOption.WaitInvoke, i);//测试10k数据
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

        public static async Task StartBigStringClient(int count)
        {
            var client =await GetClient();
            var timeSpan = TimeMeasurer.Run(async () =>
            {
                var actor = client.GetDmtpRpcActor();
                for (var i = 0; i < count; i++)
                {
                    var rs =await actor.InvokeTAsync<string>("GetBigString", InvokeOption.WaitInvoke);
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

    public partial class TestController : SingletonRpcServer
    {
        [DmtpRpc(MethodInvoke = true)]
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
}