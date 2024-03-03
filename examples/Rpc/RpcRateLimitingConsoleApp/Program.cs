using System.ComponentModel;
using System.Threading.RateLimiting;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Rpc.RateLimiting;
using TouchSocket.Sockets;

namespace RpcRateLimitingConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = new TcpDmtpService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(7789)
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();

                       a.AddRpcStore(store =>
                       {
                           store.RegisterServer<MyRpcServer>();//注册服务
                       });

                       a.AddRateLimiter(p =>
                       {
                           //添加一个名称为FixedWindow的固定窗口的限流策略
                           p.AddFixedWindowLimiter("FixedWindow", options =>
                           {
                               options.PermitLimit = 10;
                               options.Window = TimeSpan.FromSeconds(10);
                           });

                           //添加一个名称为SlidingWindow的滑动窗口的限流策略
                           p.AddSlidingWindowLimiter("SlidingWindow", options =>
                           {
                               options.PermitLimit = 10;
                               options.Window = TimeSpan.FromSeconds(10);
                               options.SegmentsPerWindow = 5;
                           });

                           //添加一个名称为TokenBucket的令牌桶的限流策略
                           p.AddTokenBucketLimiter("TokenBucket", options =>
                           {
                               options.TokenLimit = 100;
                               options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                               options.QueueLimit = 10;
                               options.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
                               options.TokensPerPeriod = 10;
                               options.AutoReplenishment = true;
                           });

                           //添加一个名称为Concurrency的并发的限流策略
                           p.AddConcurrencyLimiter("Concurrency", options =>
                           {
                               options.PermitLimit = 10;
                               options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                               options.QueueLimit = 10;
                           });
                       });
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRpc();
                   })
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "Rpc"//连接验证口令。
                   });

            service.Setup(config);

            service.Start();

            service.Logger.Info($"{service.GetType().Name}已启动");
            Console.ReadKey();
        }
    }

    public partial class MyRpcServer : RpcServer
    {
        [EnableRateLimiting("FixedWindow")]
        [Description("登录")]//服务描述，在生成代理时，会变成注释。
        [DmtpRpc("Login")]//服务注册的函数键，此处为显式指定。默认不传参的时候，为该函数类全名+方法名的全小写。
        public bool Login(string account, string password)
        {
            if (account == "123" && password == "abc")
            {
                return true;
            }

            return false;
        }
    }
}