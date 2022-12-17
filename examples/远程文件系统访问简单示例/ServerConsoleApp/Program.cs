using System;
using System.IO;
using TouchSocket.Core;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace ServerConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
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

    internal class MyPlugin : TouchRpcPluginBase<TcpTouchRpcSocketClient>
    {
        protected override void OnRemoteAccessing(TcpTouchRpcSocketClient client, RemoteAccessingEventArgs e)
        {
            client.Logger.Info($"用户{client.GetInfo()}正在进行文件系统访问，对象：{e.AccessType}，操作：{e.AccessMode}");
            if (e.AccessMode == RemoteAccessMode.Delete)
            {
                e.IsPermitOperation = false;
                e.Handled = true;
                e.Message = "不允许删除文件";
                return;
            }
        }

        protected override void OnLoadingStream(TcpTouchRpcSocketClient client, LoadingStreamEventArgs e)
        {
            //该方法会在客户端调用LoadRemoteStream时触发。
            //此时，你可以通过e.Metadata判断客户端传递来的数据。
            //最重要的是，对e.Stream进行赋值，因为这个流数据即会被映射到客户端，以供客户端读取和写入。
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.SetLength(1024 * 1024 * 10);
            e.Stream = memoryStream;
            base.OnLoadingStream(client, e);
        }
    }
}