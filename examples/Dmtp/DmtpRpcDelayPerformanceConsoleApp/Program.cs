// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

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
        private static async Task Main(string[] args)
        {
            var service = await GetService();

            var client = await GetClient();

            var tasks = new List<Task>();

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

        private static async Task<TcpDmtpService> GetService()
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
                   .SetDmtpOption(options=>
                   {
                       options.VerifyToken = "Rpc";
                   });

            await service.SetupAsync(config);
            await service.StartAsync();

            service.Logger.Info($"{service.GetType().Name}已启动");
            return service;
        }

        private static async Task<TcpDmtpClient> GetClient()
        {
            var client = new TcpDmtpClient();
            await client.SetupAsync(new TouchSocketConfig()
                  .SetRemoteIPHost("127.0.0.1:7789")
                  .ConfigurePlugins(a =>
                  {
                      a.UseDmtpRpc();
                  })
                  .SetDmtpOption(options=>
                  {
                      options.VerifyToken = "Rpc";
                  }));
            await client.ConnectAsync();


            return client;
        }
    }

    public partial class MyRpcServer : SingletonRpcServer
    {
        private readonly Timer m_timer;
        public MyRpcServer()
        {
            this.m_timer = new Timer((s) =>
            {
                Console.WriteLine(this.m_count);
            }, default, 1000, 1000);
        }

        private int m_count = 0;

        [Description("登录")]//服务描述，在生成代理时，会变成注释。
        [DmtpRpc(InvokeKey = "Login")]//服务注册的函数键，此处为显式指定。默认不传参的时候，为该函数类全名+方法名的全小写。
        public async Task<bool> Login(string account, string password)
        {
            await Task.Delay(1000 * 3);
            Interlocked.Increment(ref this.m_count);
            if (account == "123" && password == "abc")
            {
                return true;
            }

            return false;
        }
    }
}