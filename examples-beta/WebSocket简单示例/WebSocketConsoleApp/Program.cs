using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.WebApi;

namespace WebSocketConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = new HttpService();
            service.Setup(new TouchSocketConfig()//加载配置
                .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.UseWebSocket()//添加WebSocket功能
                           .SetWSUrl("/ws")//设置url直接可以连接
                           .UseAutoPong();//当收到ping报文时自动回应pong

                    a.Add<MyWebSocketPlugin>();//MyWebSocketPlugin是继承自WebSocketPluginBase的插件。
                    a.Add<MyWSCommandLinePlugin>();
                    a.UseWebApi()
                    .ConfigureRpcStore(store =>
                    {
                        store.RegisterServer<MyServer>();
                    });
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

                    //using (ByteBlock byteBlock=new ByteBlock(1024*64))//估算一下你的数据大小
                    //{
                    //    WSDataFrame dataFrame = new WSDataFrame();
                    //    dataFrame.BuildResponse(byteBlock);
                    //    ((HttpSocketClient)client).SendWithWS(byteBlock);
                    //}
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

        class MyWebsocketVerifyPlugin : PluginBase, IWebsocketHandshakingPlugin<IHttpSocketClient>
        {
            Task IWebsocketHandshakingPlugin<IHttpSocketClient>.OnWebsocketHandshaking(IHttpSocketClient client, HttpContextEventArgs e)
            {
                throw new NotImplementedException();
            }
        }

        public class MyWebSocketPlugin : WebSocketPluginBase<HttpSocketClient>
        {
            protected override void OnConnected(HttpSocketClient client, TouchSocketEventArgs e)
            {
                Console.WriteLine("TCP连接");
                base.OnConnected(client, e);
            }

            protected override void OnHandshaking(HttpSocketClient client, HttpContextEventArgs e)
            {
                Console.WriteLine("WebSocket正在连接");
                //e.IsPermitOperation = false;表示拒绝
                base.OnHandshaking(client, e);
            }

            protected override void OnHandshaked(HttpSocketClient client, HttpContextEventArgs e)
            {
                Console.WriteLine("WebSocket成功连接");
                base.OnHandshaked(client, e);
            }

            protected override void OnDisconnected(HttpSocketClient client, DisconnectEventArgs e)
            {
                Console.WriteLine("TCP断开连接");
                base.OnDisconnected(client, e);
            }

            protected override void OnHandleWSDataFrame(HttpSocketClient client, WSDataFrameEventArgs e)
            {
                switch (e.DataFrame.Opcode)
                {
                    case WSDataType.Cont:
                        client.Logger.Info($"收到中间数据，长度为：{e.DataFrame.PayloadLength}");
                        break;

                    case WSDataType.Text:
                        client.Logger.Info(e.DataFrame.ToText());
                        client.SendWithWS("我已收到");
                        break;

                    case WSDataType.Binary:
                        if (e.DataFrame.FIN)
                        {
                            client.Logger.Info($"收到二进制数据，长度为：{e.DataFrame.PayloadLength}");
                        }
                        else
                        {
                            client.Logger.Info($"收到未结束的二进制数据，长度为：{e.DataFrame.PayloadLength}");
                        }
                        break;

                    case WSDataType.Close:
                        {
                            client.Logger.Info("远程请求断开");
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
    }




}