using System;
using TouchSocket.Core.Config;
using TouchSocket.Core.Plugins;
using TouchSocket.Sockets;
using TouchSocket.Sockets.Plugins;

namespace PluginConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TcpService service = new TcpService();

            service.Connecting += (client, e) =>
            {
                client.SetDataHandlingAdapter(new TerminatorPackageAdapter("\r\n"));//命令行中使用\r\n结尾)
                //new NormalDataHandlingAdapter();//亦或者省略\r\n，但此时调用方不能高速调用，会粘包
            };

            //声明配置
            var config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) }) //同时监听两个地址
                .UsePlugin();

            //载入配置
            service.Setup(config);

            service.AddPlugin<MyCommandLinePlugin>();

            //启动
            service.Start();

            Console.WriteLine($"TCP命令行插件服务器启动成功");

            Console.ReadKey();
        }
    }

    internal class MyCommandLinePlugin : TcpCommandLinePlugin
    {
        public int AddCommand(int a, int b)
        {
            return a + b;
        }
    }

    class MyPlugin : TcpPluginBase
    {
        protected override void OnConnecting(ITcpClientBase client, ClientOperationEventArgs e)
        {
            client.SetDataHandlingAdapter(new NormalDataHandlingAdapter());
            base.OnConnecting(client, e);
        }
    }
}