using System;
using TouchSocket.Core;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace FileServiceConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TcpTouchRpcService service = GetService();
            service.Logger.Info("服务器成功启动");
            Console.ReadKey();
        }

        private static TcpTouchRpcService GetService()
        {
            var service = new TouchSocketConfig()//配置
                .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                .UsePlugin()
                .ConfigureContainer(a =>
                {
                    a.SetSingletonLogger<LoggerGroup<ConsoleLogger, FileLogger>>();
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<MyPlugin>();
                    a.Add<MyTcpPlugin>();
                })
                .SetVerifyToken("File")//连接验证口令。
                .BuildWithTcpTouchRpcService();//此处build相当于new TcpTouchRpcService，然后Setup，然后Start。

            return service;
        }
    }

    internal class MyPlugin : TouchRpcPluginBase<TcpTouchRpcSocketClient>
    {
        protected override void OnFileTransfering(TcpTouchRpcSocketClient client, FileOperationEventArgs e)
        {
            //有可能是上传，也有可能是下载
            client.Logger.Info($"有客户端请求传输文件，ID={client.ID}，请求类型={e.TransferType}，请求文件名={e.ResourcePath}");
            base.OnFileTransfering(client, e);
        }

        protected override void OnFileTransfered(TcpTouchRpcSocketClient client, FileTransferStatusEventArgs e)
        {
            //传输结束，但是不一定成功，需要从e.Result判断状态。
            client.Logger.Info($"客户端传输文件结束，ID={client.ID}，请求类型={e.TransferType}，文件名={e.ResourcePath}，请求状态={e.Result}");
            base.OnFileTransfered(client, e);
        }

        protected override void OnHandshaked(TcpTouchRpcSocketClient client, VerifyOptionEventArgs e)
        {
            client.Logger.Info($"有客户端成功验证，ID={client.ID}");
            base.OnHandshaked(client, e);
        }
    }

    internal class MyTcpPlugin : TcpPluginBase
    {
        protected override void OnDisconnected(ITcpClientBase client, ClientDisconnectedEventArgs e)
        {
            client.Logger.Info($"有客户端断开，ID={((TcpTouchRpcSocketClient)client).ID}");
            base.OnDisconnected(client, e);
        }
    }
}