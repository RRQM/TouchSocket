using System.ComponentModel;
using System.Diagnostics;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace RpcDelayPerConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var service = GetService();

            var client = GetClient();

            List<Task> tasks = new List<Task>();

            Console.WriteLine("按任意键开始");
            Console.ReadKey();

            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < 10000; i++)
            {
                tasks.Add(client.GetDmtpRpcActor().InvokeTAsync<bool>("Login", InvokeOption.WaitInvoke, "123", "abc"));
            }
            await Task.WhenAll(tasks);
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);

            while (true)
            {
                Console.ReadKey();
                tasks.Clear();
                BytePool.Default.Clear();
                GC.Collect();
            }

        }

        static TcpDmtpService GetService()
        {
            var service = new TcpDmtpService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(7789)
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRpc()
                       .ConfigureRpcStore(store =>
                       {
                           store.RegisterServer<MyRpcServer>();//注册服务
                       });
                   })
                   .SetVerifyToken("Rpc");

            service.Setup(config)
                .Start();

            service.Logger.Info($"{service.GetType().Name}已启动");
            return service;
        }

        static TcpDmtpClient GetClient()
        {
            var client = new TcpDmtpClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .ConfigurePlugins(a =>
                {
                    a.UseDmtpRpc();
                })
                .SetVerifyToken("Rpc"));
            client.Connect();


            return client;
        }
    }

    public class MyRpcServer : RpcServer
    {
        Timer m_timer;
        public MyRpcServer()
        {
            m_timer = new Timer((s) =>
            {
                Console.WriteLine(m_count);
            }, default, 1000, 1000);
        }

        int m_count = 0;

        [Description("登录")]//服务描述，在生成代理时，会变成注释。
        [DmtpRpc("Login")]//服务注册的函数键，此处为显式指定。默认不传参的时候，为该函数类全名+方法名的全小写。
        public async Task<bool> Login(string account, string password)
        {
            await Task.Delay(1000 * 3);
            Interlocked.Increment(ref m_count);
            if (account == "123" && password == "abc")
            {
                return true;
            }

            return false;
        }
    }
}