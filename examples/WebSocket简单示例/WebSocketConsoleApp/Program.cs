using System;
using TouchSocket.Core;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Http.WebSockets.Plugins;
using TouchSocket.Sockets;
using TouchSocket.Core.Plugins;
namespace WebSocketConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new HttpService();
            service.Setup(new TouchSocketConfig()//加载配置
                .UsePlugin()
                .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                .ConfigureContainer(a =>
                {
                    a.SetSingletonLogger<ConsoleLogger>();
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<WebSocketServerPlugin>()//添加WebSocket功能
                           .SetWSUrl("/ws")
                           .SetCallback(WSCallback);//WSCallback回调函数是在WS收到数据时触发回调的。
                    a.Add<MyWebSocketPlugin>();//MyWebSocketPlugin是继承自WebSocketPluginBase的插件。
                }))
                .Start();

            Console.WriteLine("服务器已启动");
            Console.WriteLine("ws://127.0.0.1:7789/ws");

            Console.ReadKey();
        }

        private static void WSCallback(ITcpClientBase client, WSDataFrameEventArgs e)
        {
            switch (e.DataFrame.Opcode)
            {
                case WSDataType.Cont:
                    Console.WriteLine($"收到中间数据，长度为：{e.DataFrame.PayloadLength}");
                    break;

                case WSDataType.Text:
                    Console.WriteLine(e.DataFrame.ToText());
                    ((HttpSocketClient)client).SendWithWS(e.DataFrame.ToText());
                    break;

                case WSDataType.Binary:
                    if (e.DataFrame.FIN)
                    {
                        Console.WriteLine($"收到二进制数据，长度为：{e.DataFrame.PayloadLength}");
                    }
                    else
                    {
                        Console.WriteLine($"收到未结束的二进制数据，长度为：{e.DataFrame.PayloadLength}");
                    }
                    break;

                case WSDataType.Close:
                    {
                        Console.WriteLine("远程请求断开");
                        client.Close("断开");
                    }

                    break;

                case WSDataType.Ping:
                    break;

                case WSDataType.Pong:
                    break;

                default:
                    break;
            }
        }
    }

    internal class MyWebSocketPlugin : WebSocketPluginBase
    {
        protected override void OnConnected(ITcpClientBase client, TouchSocketEventArgs e)
        {
            Console.WriteLine("TCP连接");
            base.OnConnected(client, e);
        }

        protected override void OnHandshaking(ITcpClientBase client, HttpContextEventArgs e)
        {
            Console.WriteLine("WebSocket正在连接");
            //e.IsPermitOperation = false;表示拒绝
            base.OnHandshaking(client, e);
        }

        protected override void OnHandshaked(ITcpClientBase client, HttpContextEventArgs e)
        {
            Console.WriteLine("WebSocket成功连接");
            base.OnHandshaked(client, e);
        }

        protected override void OnDisconnected(ITcpClientBase client, ClientDisconnectedEventArgs e)
        {
            Console.WriteLine("TCP断开连接");
            base.OnDisconnected(client, e);
        }
    }

    /// <summary>
    /// 命令行插件。
    /// 声明的方法必须以"Command"结尾，支持json字符串，参数之间空格隔开。
    /// </summary>
    internal class MyWSCommandLinePlugin : WSCommandLinePlugin
    {
        public int AddCommand(int a, int b)
        {
            return a + b;
        }

        public SumClass SumCommand(SumClass sumClass)
        {
            sumClass.Sum = sumClass.A + sumClass.B;
            return sumClass;
        }
    }

    internal class SumClass
    {
        public int A { get; set; }
        public int B { get; set; }
        public int Sum { get; set; }
    }
}
