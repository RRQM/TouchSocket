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
            var service = CreateHttpService();
            ConnectWith_ws();
            ConnectWith_wsquery();
            ConnectWith_wsheader();
            Console.ReadKey();
        }

        /// <summary>
        /// 通过/ws直接连接
        /// </summary>
        static void ConnectWith_ws()
        {
            using var client = new WebSocketClient();
            client.Setup(new TouchSocketConfig()
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .SetRemoteIPHost("ws://127.0.0.1:7789/ws"));
            client.Connect();

            client.Logger.Info("通过ws://127.0.0.1:7789/ws连接成功");
        }

        /// <summary>
        /// 通过/wsquery，传入参数连接
        /// </summary>
        static void ConnectWith_wsquery()
        {
            using var client = new WebSocketClient();
            client.Setup(new TouchSocketConfig()
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .SetRemoteIPHost("ws://127.0.0.1:7789/wsquery?token=123456"));
            client.Connect();

            client.Logger.Info("通过ws://127.0.0.1:7789/wsquery?token=123456连接成功");
        }

        /// <summary>
        /// 通过/wsheader,传入header连接
        /// </summary>
        static void ConnectWith_wsheader()
        {
            using var client = new WebSocketClient();
            client.Setup(new TouchSocketConfig()
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.Add(nameof(IWebSocketHandshakingPlugin.OnWebSocketHandshaking), async (IHttpClientBase client, HttpContextEventArgs e) =>
                    {
                        e.Context.Request.Headers.Add("token", "123456");
                        await e.InvokeNext();
                    });
                })
                .SetRemoteIPHost("ws://127.0.0.1:7789/wsheader"));
            client.Connect();

            client.Logger.Info("通过ws://127.0.0.1:7789/wsheader连接成功");
        }

        static HttpService CreateHttpService()
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
                                    //.SetWSUrl("/ws")//设置url直接可以连接。
                           .SetVerifyConnection(VerifyConnection)
                           .UseAutoPong();//当收到ping报文时自动回应pong

                    a.Add<MyWebSocketPlugin<IHttpSocketClient>>();
                    a.Add<MyWSCommandLinePlugin>();
                    a.UseWebApi()
                    .ConfigureRpcStore(store =>
                    {
                        store.RegisterServer<MyServer>();
                    });
                }))
                .Start();

            service.Logger.Info("服务器已启动");
            service.Logger.Info("直接连接地址=>ws://127.0.0.1:7789/ws");
            service.Logger.Info("通过query连接地址=>ws://127.0.0.1:7789/wsquery?token=123456");
            service.Logger.Info("通过header连接地址=>ws://127.0.0.1:7789/wsheader");//使用此连接时，需要在header中包含token的项
            service.Logger.Info("WebApi支持的连接地址=>ws://127.0.0.1:7789/MyServer/ConnectWS");
            service.Logger.Info("WebApi支持的连接地址=>ws://127.0.0.1:7789/MyServer/ws");
            return service;
        }

        /// <summary>
        /// 验证websocket的连接
        /// </summary>
        /// <param name="client"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static bool VerifyConnection(IHttpSocketClient client, HttpContext context)
        {
            if (!context.Request.IsUpgrade())//如果不包含升级协议的header，就直接返回false。
            {
                return false;
            }
            if (context.Request.UrlEquals("/ws"))//以此连接，则直接可以连接
            {
                return true;
            }
            else if (context.Request.UrlEquals("/wsquery"))//以此连接，则需要传入token才可以连接
            {
                if (context.Request.Query.Get("token") == "123456")
                {
                    return true;
                }
                else
                {
                    context.Response
                        .SetStatus(403, "token不正确")
                        .Answer();
                }
            }
            else if (context.Request.UrlEquals("/wsheader"))//以此连接，则需要从header传入token才可以连接
            {
                if (context.Request.Headers.Get("token") == "123456")
                {
                    return true;
                }
                else
                {
                    context.Response
                        .SetStatus(403, "token不正确")
                        .Answer();
                }
            }
            return false;
        }

        public class MyWebSocketPlugin<TSocketClient> : PluginBase, IWebSocketHandshakingPlugin<TSocketClient>,
            IWebSocketHandshakedPlugin<TSocketClient>, IWebSocketReceivedPlugin<TSocketClient> where TSocketClient : IHttpSocketClient
        {
            async Task IWebSocketHandshakingPlugin<TSocketClient>.OnWebSocketHandshaking(TSocketClient client, HttpContextEventArgs e)
            {
                client.Logger.Info("WebSocket正在连接");
                await e.InvokeNext();
            }

            async Task IWebSocketHandshakedPlugin<TSocketClient>.OnWebSocketHandshaked(TSocketClient client, HttpContextEventArgs e)
            {
                client.Logger.Info("WebSocket成功连接");
                await e.InvokeNext();
            }

            async Task IWebSocketReceivedPlugin<TSocketClient>.OnWebSocketReceived(TSocketClient client, WSDataFrameEventArgs e)
            {
                switch (e.DataFrame.Opcode)
                {
                    case WSDataType.Cont:
                        client.Logger.Info($"收到中间数据，长度为：{e.DataFrame.PayloadLength}");

                        return;

                    case WSDataType.Text:
                        client.Logger.Info(e.DataFrame.ToText());
                        client.SendWithWS("我已收到");
                        return;

                    case WSDataType.Binary:
                        if (e.DataFrame.FIN)
                        {
                            client.Logger.Info($"收到二进制数据，长度为：{e.DataFrame.PayloadLength}");
                        }
                        else
                        {
                            client.Logger.Info($"收到未结束的二进制数据，长度为：{e.DataFrame.PayloadLength}");
                        }
                        return;

                    case WSDataType.Close:
                        {
                            client.Logger.Info("远程请求断开");
                            client.Close("断开");
                        }
                        return;

                    case WSDataType.Ping:
                        break;

                    case WSDataType.Pong:
                        break;

                    default:
                        break;
                }

                await e.InvokeNext();
            }
        }

        /// <summary>
        /// 命令行插件。
        /// 声明的方法必须以"Command"结尾，支持json字符串，参数之间空格隔开。
        /// </summary>
        public class MyWSCommandLinePlugin : WebSocketCommandLinePlugin
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