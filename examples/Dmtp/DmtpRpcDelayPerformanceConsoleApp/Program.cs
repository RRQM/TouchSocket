using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
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
            var service = await GetService();

            var client = await GetClient();

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
                GC.Collect();
            }

        }

        static async Task<TcpDmtpService> GetService()
        {
            var service = new TcpDmtpService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(7789)
                   .ConfigureContainer(a =>
                   {
                       a.AddRpcStore(store =>
                       {
                           store.RegisterServer<MyRpcServer>();
                       });
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRpc();
                   })
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "Rpc"
                   });

            await service.SetupAsync(config);
            await service.StartAsync();

            service.Logger.Info($"{service.GetType().Name}已启动");
            return service;
        }

        static async Task<TcpDmtpClient> GetClient()
        {
            var client = new TcpDmtpClient();
            await client.SetupAsync(new TouchSocketConfig()
                  .SetRemoteIPHost("127.0.0.1:7789")
                  .ConfigurePlugins(a =>
                  {
                      a.UseDmtpRpc();
                  })
                  .SetDmtpOption(new DmtpOption()
                  {
                      VerifyToken = "Rpc"
                  }));
            await client.ConnectAsync();


            return client;
        }
    }

    public partial class MyRpcServer : SingletonRpcServer
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
        [DmtpRpc(InvokeKey = "Login")]//服务注册的函数键，此处为显式指定。默认不传参的时候，为该函数类全名+方法名的全小写。
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