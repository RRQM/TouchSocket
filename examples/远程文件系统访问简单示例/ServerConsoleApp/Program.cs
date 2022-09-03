using System;
using TouchSocket.Core;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Core.Plugins;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Rpc.TouchRpc.Plugins;
using TouchSocket.Sockets;

namespace ServerConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Enterprise.ForTest();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var service = new TcpTouchRpcService();
            TouchSocketConfig config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                   .UsePlugin()
                   .ConfigurePlugins(a =>
                   {
                       a.Add<MyPlugin>();
                   })
                   .ConfigureContainer(a =>
                   {
                       a.SetLogger<LoggerGroup<ConsoleLogger, FileLogger>>();//注册一个日志组
                   })
                   .SetVerifyToken("TouchRpc");

            service.Setup(config)
                .Start();

            service.Logger.Info($"{service.GetType().Name}已启动，监听端口：{7789}");
            Console.ReadKey();
        }
    }

    class MyPlugin : TouchRpcPluginBase<TcpTouchRpcSocketClient>
    {
        protected override void OnRemoteAccessing(TcpTouchRpcSocketClient client, RemoteAccessActionEventArgs e)
        {
            client.Logger.Info($"用户{client.GetInfo()}正在进行文件系统访问，对象：{e.AccessType}，操作：{e.AccessMode}");
            if (e.AccessMode== RemoteAccessMode.Delete)
            {
                e.IsPermitOperation = false;
                e.Handled = true;
                e.Message = "不允许删除文件";
                return;
            }
            base.OnRemoteAccessing(client, e);
        }
    }
}
