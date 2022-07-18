using System;
using TouchSocket.Core;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Core.Plugins;
using TouchSocket.Http;
using TouchSocket.Http.Plugins;
using TouchSocket.Http.WebSockets;
using TouchSocket.Http.WebSockets.Plugins;
using TouchSocket.Sockets;

namespace ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //证书在RRQMBox/Ssl证书相关/证书生成.zip  解压获取。
            //然后放在运行目录。
            //最后客户端需要先安装证书。

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
                    a.Add<MyHttpPlug>();
                    a.Add<WebSocketServerPlugin>()//添加WebSocket功能
                           .SetWSUrl("/ws")
                           .SetCallback(WSCallback);//WSCallback回调函数是在WS收到数据时触发回调的。
                    a.Add<MyWebSocketPlugin>();
                    a.Add<MyWSCommandLinePlugin>();
                }))
                .Start();

            Console.WriteLine("Http服务器已启动");
            Console.WriteLine("访问 http://127.0.0.1:7789/success 返回响应");
            Console.WriteLine("访问 http://127.0.0.1:7789/file 响应文件");
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

    /// <summary>
    /// 支持GET、Post、Put，Delete，或者其他
    /// </summary>
    internal class MyHttpPlug : HttpPluginBase
    {
        protected override void OnGet(ITcpClientBase client, HttpContextEventArgs e)
        {
            if (e.Context.Request.UrlEquals("/success"))
            {
                e.Context.Response.FromText("Success").Answer();//直接回应
                Console.WriteLine("处理完毕");
                e.Handled = true;
            }
            else if (e.Context.Request.UrlEquals("/file"))
            {
                e.Context.Response
                    .SetStatus()//必须要有状态
                    .FromFile(@"D:\System\Windows.iso", e.Context.Request);//直接回应文件。
            }
            base.OnGet(client, e);
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