using System.Text;
using TouchSocket.Core;
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace NamedPipeClientConsoleApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var client = await CreateClient();

            while (true)
            {
                await client.SendAsync(Console.ReadLine());
            }
        }

        private static async Task<NamedPipeClient> CreateClient()
        {
            var client = new NamedPipeClient();
            client.Connected = (client, e) =>
            {
                return Task.CompletedTask;
            };//成功连接到服务器

            client.Closed = (client, e) =>
            {
                return Task.CompletedTask;
            };//从服务器断开连接，当连接不成功时不会触发。

            client.Received = (client, e) =>
            {
                //从服务器收到信息
                var mes = e.ByteBlock.Span.ToString(Encoding.UTF8);
                client.Logger.Info($"客户端接收到信息：{mes}");

                return Task.CompletedTask;
            };

            //载入配置
            await client.SetupAsync(new TouchSocketConfig()
                 .SetPipeServer(".")//一般本机管道时，可以不用此配置
                 .SetPipeName("touchsocketpipe")//管道名称
                 .ConfigurePlugins(a =>
                 {
                 })
                 .ConfigureContainer(a =>
                 {
                     a.AddConsoleLogger();//添加一个日志注入
                 }));
            await client.ConnectAsync();
            client.Logger.Info("客户端成功连接");
            return client;
        }
    }
}