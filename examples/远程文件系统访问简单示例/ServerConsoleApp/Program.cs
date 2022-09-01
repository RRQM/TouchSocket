using System;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace ServerConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new TcpTouchRpcService();
            TouchSocketConfig config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                   .ConfigureContainer(a =>
                   {
                       a.SetLogger<LoggerGroup<ConsoleLogger, FileLogger>>();//注册一个日志组
                   })
                   .SetVerifyToken("TouchRpc");

            service.Setup(config)
                .Start();

            service.Logger.Info($"{service.GetType().Name}已启动，监听端口：{7789}");
        }
    }
}
