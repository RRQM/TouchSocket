using System.Net.Sockets;
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
            var client = new NamedPipeClient();
            client.Connected = (client, e) =>
            {
                return Task.CompletedTask;
            };//成功连接到服务器

            client.Disconnected = (client, e) =>
            {
                return Task.CompletedTask;
            };//从服务器断开连接，当连接不成功时不会触发。

            client.Received = (client, e) =>
            {
                //从服务器收到信息
                var mes = Encoding.UTF8.GetString(e.ByteBlock.Buffer, 0, e.ByteBlock.Len);
                client.Logger.Info($"客户端接收到信息：{mes}");

                return Task.CompletedTask;
            };

            //载入配置
            client.Setup(new TouchSocketConfig()
                .SetPipeServer(".")//一般本机管道时，可以不用此配置
                .SetPipeName("touchsocketpipe")//管道名称
                .ConfigurePlugins(a =>
                {

                })
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();//添加一个日志注入
                }));
            client.Connect();
            client.Logger.Info("客户端成功连接");
            return client;
        }
    }
}