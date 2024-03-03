using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Redis;
using TouchSocket.Sockets;

namespace DmtpRedisConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = GetTcpDmtpService();
            var client = GetTcpDmtpClient();

            //获取Redis
            var redis = client.GetDmtpRedisActor();

            //执行Set
            var result = redis.Set("1", "1");
            client.Logger.Info($"Set result={result}");
            client.Logger.Info($"ContainsCache result={redis.ContainsCache("1")}");

            //执行Get
            var result1 = redis.Get<string>("1");
            client.Logger.Info($"Get result={result}");

            //执行Remove
            result = redis.RemoveCache("1");
            client.Logger.Info($"Get result={result}");
            redis.ClearCache();

            Console.ReadKey();
        }

        private static TcpDmtpClient GetTcpDmtpClient()
        {
            var client = new TcpDmtpClient();
            client.Setup(new TouchSocketConfig()
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
            client.Connect();
            return client;
        }

        private static TcpDmtpService GetTcpDmtpService()
        {
            var service = new TouchSocketConfig()//配置
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
                   .BuildWithTcpDmtpService();//此处build相当于new TcpDmtpService，然后Setup，然后Start。
            service.Logger.Info("服务器成功启动");
            return service;
        }
    }
}