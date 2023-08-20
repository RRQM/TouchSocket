using System.Text;
using TouchSocket.Core;
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace NamedPipeClientConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var client = CreateClient();

            while (true) 
            {
                client.Send(Console.ReadLine());
            }
        }

        private static NamedPipeClient CreateClient()
        {
            var tcpClient = new NamedPipeClient();
            tcpClient.Connected = (client, e) => { };//成功连接到服务器
            tcpClient.Disconnected = (client, e) => { };//从服务器断开连接，当连接不成功时不会触发。
            tcpClient.Received = (client, byteBlock, requestInfo) =>
            {
                //从服务器收到信息
                var mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                tcpClient.Logger.Info($"客户端接收到信息：{mes}");
            };

            //载入配置
            tcpClient.Setup(new TouchSocketConfig()
                .SetPipeServer(".")//一般本机管道时，可以不用此配置
                .SetPipeName("TouchSocketPipe")//管道名称
                .ConfigurePlugins(a =>
                {
                    
                })
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();//添加一个日志注入
                }))
                .Connect();
            tcpClient.Logger.Info("客户端成功连接");
            return tcpClient;
        }
    }
}