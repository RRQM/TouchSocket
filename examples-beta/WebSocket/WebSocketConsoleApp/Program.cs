﻿using System;
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

            var consoleAction = new ConsoleAction();
            consoleAction.OnException += ConsoleAction_OnException;

            consoleAction.Add("1", "使用/ws直接连接", ConnectWith_ws);
            consoleAction.Add("2", "使用/ws和query参数连接", ConnectWith_wsquery);
            consoleAction.Add("3", "使用/ws和header参数连接", ConnectWith_wsheader);
            consoleAction.Add("4", "使用post方式连接", ConnectWith_Post_ws);
            consoleAction.Add("5", "发送字符串", SendText);
            consoleAction.Add("6", "调用Add", SendAdd);

            consoleAction.ShowAll();
            consoleAction.RunCommandLine();

        }

        private static async void SendAdd()
        {
            var client = new WebSocketClient();
            client.Setup(new TouchSocketConfig()
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.Add(nameof(IWebSocketReceivedPlugin.OnWebSocketReceived), async (IHttpClientBase c, WSDataFrameEventArgs e) =>
                    {
                        client.Logger.Info($"收到Add的计算结果：{e.DataFrame.ToText()}");
                        await e.InvokeNext();
                    });
                })
                .SetRemoteIPHost("ws://127.0.0.1:7789/ws"));
            client.Connect();

            client.SendWithWS("Add 10 20");
            await Task.Delay(1000);

        }
        private static void SendText()
        {
            var client = new WebSocketClient();
            client.Setup(new TouchSocketConfig()
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .SetRemoteIPHost("ws://127.0.0.1:7789/ws"));
            client.Connect();
            client.SendWithWS("hello");
        }

        private static void ConsoleAction_OnException(Exception obj)
        {
            Console.WriteLine(obj.Message);
        }

        /// <summary>
        /// 通过/ws直接连接
        /// </summary>
        private static void ConnectWith_ws()
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
        private static void ConnectWith_wsquery()
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
        private static void ConnectWith_wsheader()
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

        /// <summary>
        /// 使用Post方式连接
        /// </summary>
        private static void ConnectWith_Post_ws()
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
                        e.Context.Request.Method = HttpMethod.Post;//将请求方法改为Post
                        await e.InvokeNext();
                    });
                })
                .SetRemoteIPHost("ws://127.0.0.1:7789/postws"));
            client.Connect();

            client.Logger.Info("通过ws://127.0.0.1:7789/postws连接成功");
        }

        private static HttpService CreateHttpService()
        {
            var service = new HttpService();
            service.Setup(new TouchSocketConfig()//加载配置
                .SetListenIPHosts(7789)
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

                    a.Add<MyWSCommandLinePlugin>();
                    a.Add<MyWebSocketPlugin>();

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

            //使用Post连接
            if (context.Request.Method == HttpMethod.Post)
            {
                if (context.Request.UrlEquals("/postws"))
                {
                    return true;
                }
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


        public class MyWebSocketPlugin : PluginBase, IWebSocketHandshakingPlugin, IWebSocketHandshakedPlugin, IWebSocketReceivedPlugin
        {
            async Task IWebSocketHandshakingPlugin<IHttpClientBase>.OnWebSocketHandshaking(IHttpClientBase client, HttpContextEventArgs e)
            {
                client.Logger.Info("WebSocket正在连接");
                await e.InvokeNext();
            }

            async Task IWebSocketHandshakedPlugin<IHttpClientBase>.OnWebSocketHandshaked(IHttpClientBase client, HttpContextEventArgs e)
            {
                client.Logger.Info("WebSocket成功连接");
                await e.InvokeNext();
            }

            async Task IWebSocketReceivedPlugin<IHttpClientBase>.OnWebSocketReceived(IHttpClientBase client, WSDataFrameEventArgs e)
            {
                switch (e.DataFrame.Opcode)
                {
                    case WSDataType.Cont:
                        client.Logger.Info($"收到中间数据，长度为：{e.DataFrame.PayloadLength}");

                        return;

                    case WSDataType.Text:
                        client.Logger.Info(e.DataFrame.ToText());

                        if (!client.IsClient)
                        {
                            client.SendWithWS("我已收到");
                        }
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

        class MyHttpPlugin : PluginBase, IHttpGetPlugin<IHttpSocketClient>
        {
            async Task IHttpGetPlugin<IHttpSocketClient>.OnHttpGet(IHttpSocketClient client, HttpContextEventArgs e)
            {
                if (e.Context.Request.UrlEquals("/GetSwitchToWebSocket"))
                {
                    var result = client.SwitchProtocolToWebSocket(e.Context);
                    return;
                }
                await e.InvokeNext();
            }
        }

        /// <summary>
        /// 命令行插件。
        /// 声明的方法必须为公开实例方法、以"Command"结尾，且支持json字符串，参数之间空格隔开。
        /// </summary>
        public class MyWSCommandLinePlugin : WebSocketCommandLinePlugin
        {
            public MyWSCommandLinePlugin(ILog logger) : base(logger)
            {
            }

            public int AddCommand(int a, int b)
            {
                return a + b;
            }

            //当第一个参数，直接或间接实现ITcpClientBase接口时，会收集到当前请求的客户端，从而可以获取IP等。
            public SumClass SumCommand(IHttpClientBase client, SumClass sumClass)
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
                        this.m_logger.Info("WS通过WebApi连接");
                    }
                }
            }
        }
    }
}