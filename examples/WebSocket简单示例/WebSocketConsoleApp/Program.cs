using System;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Rpc;
using TouchSocket.Rpc.WebApi;
using TouchSocket.Sockets;

namespace WebSocketConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = new HttpService();
            service.Setup(new TouchSocketConfig()//加载配置
                .UsePlugin()
                .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .ConfigureRpcStore(a =>
                {
                    a.RegisterServer<MyServer>();
                })
                .ConfigurePlugins(a =>
                {
                    a.UseWebSocket()//添加WebSocket功能
                           .SetWSUrl("/ws")
                           .SetCallback(WSCallback);//WSCallback回调函数是在WS收到数据时触发回调的。
                    a.Add<MyWebSocketPlugin>();//MyWebSocketPlugin是继承自WebSocketPluginBase的插件。
                    a.Add<MyWSCommandLinePlugin>();
                    a.UseWebApi();
                }))
                .Start();

            Console.WriteLine("服务器已启动，可使用下列地址连接");
            Console.WriteLine("ws://127.0.0.1:7789/ws");
            Console.WriteLine("ws://127.0.0.1:7789/MyServer/ConnectWS");
            Console.WriteLine("ws://127.0.0.1:7789/MyServer/ws");

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

    public class MyServer : RpcServer
    {
        private readonly ILog m_logger;

        public MyServer(ILog logger)
        {
            this.m_logger = logger;
        }

        [Router("/[api]/ws")]
        [Router("/[api]/[action]")]
        [WebApi(HttpMethodType.GET, MethodFlags = MethodFlags.IncludeCallContext)]
        public void ConnectWS(IWebApiCallContext callContext)
        {
            if (callContext.Caller is HttpSocketClient socketClient)
            {
                if (socketClient.SwitchProtocolToWebSocket(callContext.HttpContext))
                {
                    m_logger.Info("WS通过WebApi连接");
                }
            }
        }
    }

    public class MyWebSocketPlugin : WebSocketPluginBase
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
    public class MyWSCommandLinePlugin : WSCommandLinePlugin
    {
        public MyWSCommandLinePlugin(ILog logger) : base(logger)
        {
        }

        public int AddCommand(IHttpClientBase client, int a, int b)
        {
            return a + b;
        }

        public SumClass SumCommand(SumClass sumClass)
        {
            sumClass.Sum = sumClass.A + sumClass.B;
            return sumClass;
        }
    }

    public class SumClass
    {
        public int A { get; set; }
        public int B { get; set; }
        public int Sum { get; set; }
    }
}