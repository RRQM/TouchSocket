using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Redis;
using TouchSocket.Sockets;

namespace DmtpRedisConsoleApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var service = await GetTcpDmtpService();
            var client = await GetTcpDmtpClient();

            //获取Redis
            var redis = client.GetDmtpRedisActor();

            //执行Set
            var result = await redis.SetAsync("1", "1");
            client.Logger.Info($"Set result={result}");
            client.Logger.Info($"ContainsCache result={await redis.ContainsCacheAsync("1")}");

            //执行Get
            var result1 = await redis.GetAsync<string>("1");
            client.Logger.Info($"Get result={result}");

            //执行Remove
            result = await redis.RemoveCacheAsync("1");
            client.Logger.Info($"Get result={result}");
            await redis.ClearCacheAsync();

            Console.ReadKey();
        }

        private static async Task<TcpDmtpClient> GetTcpDmtpClient()
        {
            var client = new TcpDmtpClient();
            await client.SetupAsync(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetDmtpOption(new DmtpOption()
                {
                    VerifyToken = "Dmtp"
                })
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.UseDmtpRedis();
                }));
            await client.ConnectAsync();
            return client;
        }

        private static async Task<TcpDmtpService> GetTcpDmtpService()
        {
            var service = await new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRedis()//必须添加Redis访问插件
                       .SetCache(new MemoryCache<string, byte[]>());//这里可以设置缓存持久化，此处仍然是使用内存缓存。
                   })
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "Dmtp"//连接验证口令。
                   })
                   .BuildServiceAsync<TcpDmtpService>();//此处build相当于new TcpDmtpService，然后SetupAsync，然后StartAsync。
            service.Logger.Info("服务器成功启动");
            return service;
        }
    }
}