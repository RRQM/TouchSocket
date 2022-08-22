using System;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace FileServiceConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpTouchRpcService service = GetService();
            service.Logger.Info("服务器成功启动");
            Console.ReadKey();
        }

        static TcpTouchRpcService GetService()
        {
            var service = new TouchSocketConfig()//配置     
                .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                .ConfigureContainer(a =>
                {
                    a.SetSingletonLogger<LoggerGroup<ConsoleLogger, FileLogger>>();
                })
                .SetVerifyToken("File")//连接验证口令。
                .BuildWithTcpTouchRpcService();//此处build相当于new TcpTouchRpcService，然后Setup，然后Start。

            service.Handshaked += (client, e) =>
            {
                client.Logger.Info($"有客户端成功验证，ID={client.ID}");
            };

            service.Disconnected += (client, e) =>
            {
                client.Logger.Info($"有客户端断开，ID={client.ID}");
            };

            service.FileTransfering += (client, e) =>
            {
                //有可能是上传，也有可能是下载
                client.Logger.Info($"有客户端请求传输文件，ID={client.ID}，请求类型={e.TransferType}，请求文件名={e.FileRequest.Path}");
            };

            service.FileTransfered += (client, e) =>
            {
                //传输结束，但是不一定成功，需要从e.Result判断状态。
                client.Logger.Info($"客户端传输文件结束，ID={client.ID}，请求类型={e.TransferType}，文件名={e.FileRequest.Path}，请求状态={e.Result}");
            };

            return service;
        }
    }
}
