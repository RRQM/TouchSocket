//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.WebApi;

namespace WebSocketConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var consoleAction = new ConsoleAction();
        consoleAction.OnException += ConsoleAction_OnException;

        consoleAction.Add("1", "使用/ws直接连接", ConnectWith_ws);
        consoleAction.Add("2", "使用/ws和query参数连接", ConnectWith_wsquery);
        consoleAction.Add("3", "使用/ws和header参数连接", ConnectWith_wsheader);
        consoleAction.Add("4", "使用post方式连接", ConnectWith_Post_ws);
        consoleAction.Add("5", "发送字符串", SendText);
        consoleAction.Add("6", "发送部分字符串", SendSubstringText);
        consoleAction.Add("7", "调用Add", SendAdd);
        consoleAction.Add("8", "发送二进制", SendBinary);
        consoleAction.Add("9", "发送自定义消息", SendCustomMessage);
        consoleAction.Add("10", "发送Ping消息", SendPingMessage);
        consoleAction.Add("11", "发送分包消息", SendContMessage);

        consoleAction.ShowAll();

        var service = await CreateHttpService();

        await consoleAction.RunCommandLineAsync();
    }

    private static async Task SendContMessage()
    {
        using var webSocket = new WebSocketClient();
        await webSocket.SetupAsync(new TouchSocketConfig()
              .ConfigureContainer(a =>
              {
                  a.AddConsoleLogger();
              })
              .SetRemoteIPHost("ws://127.0.0.1:7789/ws"));
        await webSocket.ConnectAsync();

        #region WebSocket发送分包数据
        for (int i = 0; i < 10; i++)
        {
            //发送文本分包消息
            //最后一个参数表示是否为最后一个包
            await webSocket.SendAsync($"hello{i}", i == 9);
        }

        for (int i = 0; i < 10; i++)
        {
            //发送二进制分包消息
            //最后一个参数表示是否为最后一个包
            await webSocket.SendAsync(new byte[] { 0, 1, 2, 3, 4 }, i == 9);
        }
        #endregion

        #region WebSocket关闭连接
        await webSocket.CloseAsync("正常关闭");

        //或者使用关闭状态码
        await webSocket.CloseAsync( WebSocketCloseStatus.NormalClosure,"正常关闭");
        #endregion
    }

    private static async Task SendPingMessage()
    {
        using var webSocket = new WebSocketClient();
        await webSocket.SetupAsync(new TouchSocketConfig()
              .ConfigureContainer(a =>
              {
                  a.AddConsoleLogger();
              })
              .SetRemoteIPHost("ws://127.0.0.1:7789/ws"));
        await webSocket.ConnectAsync();

        for (int i = 0; i < 10; i++)
        {
            #region WebSocket发送Ping或者Pong消息
            await webSocket.PingAsync();

            //如果收到了Ping消息，则需要回应Pong。
            //await webSocket.PongAsync();
            #endregion
        }
    }

    private static async Task SendCustomMessage()
    {
        using var webSocket = new WebSocketClient();
        await webSocket.SetupAsync(new TouchSocketConfig()
              .ConfigureContainer(a =>
              {
                  a.AddConsoleLogger();
              })
              .SetRemoteIPHost("ws://127.0.0.1:7789/ws"));
        await webSocket.ConnectAsync();

        #region WebSocket发送自定义消息
        var frame = new WSDataFrame(Encoding.UTF8.GetBytes("Hello"));
        frame.Opcode = WSDataType.Text;
        frame.FIN = true;

        //设置RSV位，一般情况下，RSV位不允许随意设置，除非你非常清楚你在做什么。
        frame.RSV1 = true;
        frame.RSV2 = true;
        frame.RSV3 = true;
        await webSocket.SendAsync(frame);
        #endregion
    }

    private static async Task SendBinary()
    {
        using var webSocket = new WebSocketClient();
        await webSocket.SetupAsync(new TouchSocketConfig()
              .ConfigureContainer(a =>
              {
                  a.AddConsoleLogger();
              })
              .SetRemoteIPHost("ws://127.0.0.1:7789/ws"));
        await webSocket.ConnectAsync();

        #region WebSocket发送二进制
        await webSocket.SendAsync(new byte[] { 1, 2, 3, 4, 5 });
        #endregion
    }

    private static async Task SendAdd()
    {
        var client = new WebSocketClient();
        await client.SetupAsync(new TouchSocketConfig()
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();
             })
             .ConfigurePlugins(a =>
             {
                 a.UseReconnection<WebSocketClient>();
                 a.Add(typeof(IWebSocketReceivedPlugin), async (IHttpSession c, WSDataFrameEventArgs e) =>
                 {
                     client.Logger.Info($"收到Add的计算结果：{e.DataFrame.ToText()}");
                     await e.InvokeNext();
                 });
             })
             .SetRemoteIPHost("ws://127.0.0.1:7789/ws"));
        await client.ConnectAsync();

        await client.SendAsync("Add 10 20");
        await Task.Delay(1000);

        await client.CloseAsync("我想关就关");

        //或者使用关闭状态码
        //await client.CloseAsync( WebSocketCloseStatus.InternalServerError,"我不想关，但还得关");
    }

    private static async Task SendSubstringText()
    {
        var client = new WebSocketClient();
        await client.SetupAsync(new TouchSocketConfig()
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();
             })
             .SetRemoteIPHost("ws://127.0.0.1:7789/ws"));
        await client.ConnectAsync();

        for (var i = 0; i < 10; i++)
        {
            var msg = Encoding.UTF8.GetBytes("hello");
            await client.SendAsync("Hello", i == 9);
        }
    }

    private static async Task SendText()
    {
        using var webSocket = new WebSocketClient();
        await webSocket.SetupAsync(new TouchSocketConfig()
              .ConfigureContainer(a =>
              {
                  a.AddConsoleLogger();
              })
              .SetRemoteIPHost("ws://127.0.0.1:7789/ws"));
        await webSocket.ConnectAsync();

        #region WebSocket发送文本
        await webSocket.SendAsync("hello");
        #endregion
    }

    private static void ConsoleAction_OnException(Exception obj)
    {
        Console.WriteLine(obj.Message);
    }

    /// <summary>
    /// 通过/ws直接连接
    /// </summary>
    private static async Task ConnectWith_ws()
    {
        using var client = new WebSocketClient();
        await client.SetupAsync(new TouchSocketConfig()
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();
             })
             .ConfigurePlugins(a =>
             {
                 a.Add(typeof(IWebSocketHandshakedPlugin), () =>
                 {
                     Console.WriteLine("WebSocketHandshaked");
                 });

                 a.Add(typeof(IWebSocketClosingPlugin), () =>
                 {
                     Console.WriteLine("WebSocketClosing");
                 });

                 a.Add(typeof(IWebSocketClosedPlugin), () =>
                 {
                     Console.WriteLine("WebSocketClosed");
                 });

                 a.UseWebSocketHeartbeat()
                 .SetTick(TimeSpan.FromSeconds(1));

                 a.UseReconnection<WebSocketClient>();
             })
             .SetRemoteIPHost("ws://127.0.0.1:7789/ws"));
        await client.ConnectAsync();

        client.Logger.Info("通过ws://127.0.0.1:7789/ws连接成功");

        await Task.Delay(1000000);
    }

    /// <summary>
    /// 通过/wsquery，传入参数连接
    /// </summary>
    private static void ConnectWith_wsquery()
    {
        using var client = new WebSocketClient();
        client.SetupAsync(new TouchSocketConfig()
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
            })
            .SetRemoteIPHost("ws://127.0.0.1:7789/wsquery?token=123456"));
        client.ConnectAsync();

        client.Logger.Info("通过ws://127.0.0.1:7789/wsquery?token=123456连接成功");
    }

    /// <summary>
    /// 通过/wsheader,传入header连接
    /// </summary>
    private static void ConnectWith_wsheader()
    {
        using var client = new WebSocketClient();
        client.SetupAsync(new TouchSocketConfig()
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
            })
            .ConfigurePlugins(a =>
            {
                a.Add(typeof(IWebSocketHandshakingPlugin), async (IWebSocket webSocket, HttpContextEventArgs e) =>
                {
                    e.Context.Request.Headers.Add("token", "123456");
                    await e.InvokeNext();
                });
            })
            .SetRemoteIPHost("ws://127.0.0.1:7789/wsheader"));
        client.ConnectAsync();

        client.Logger.Info("通过ws://127.0.0.1:7789/wsheader连接成功");
    }

    /// <summary>
    /// 使用Post方式连接
    /// </summary>
    private static void ConnectWith_Post_ws()
    {
        using var client = new WebSocketClient();
        client.SetupAsync(new TouchSocketConfig()
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
            })
            .ConfigurePlugins(a =>
            {
                a.Add(typeof(IWebSocketHandshakingPlugin), async (IWebSocket webSocket, HttpContextEventArgs e) =>
                {
                    e.Context.Request.Method = HttpMethod.Post;//将请求方法改为Post
                    await e.InvokeNext();
                });
            })
            .SetRemoteIPHost("ws://127.0.0.1:7789/postws"));
        client.ConnectAsync();

        client.Logger.Info("通过ws://127.0.0.1:7789/postws连接成功");
    }
    private static async Task<HttpService> CreateHttpService2()
    {
        #region 创建WebSocket服务器 {11-16}
        var service = new HttpService();

        var config = new TouchSocketConfig();
        config.SetListenIPHosts(7789)
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();
             })
             .ConfigurePlugins(a =>
             {
                 //添加WebSocket功能
                 a.UseWebSocket(options =>
                 {
                     options.SetUrl("/ws");//设置Url，当客户端使用ws://127.0.0.1:7789/ws直接可以连接。
                     options.SetAutoPong(true);//当收到Ping报文时自动回应Pong
                 });
             });

        //加载配置
        await service.SetupAsync(config);

        //启动服务
        await service.StartAsync();

        service.Logger.Info("WebSocket服务器已启动，地址: ws://127.0.0.1:7789/ws");
        #endregion

        #region WebSocket服务器遍历所有连接并发送数据
        var clients = service.Clients;
        foreach (var client in clients)
        {
            if (client.Protocol == Protocol.WebSocket)//先判断是不是websocket协议
            {
                if (client.Id == "id")//再按指定id发送，或者直接广播发送
                {
                    var webSocket = client.WebSocket;
                    await webSocket.SendAsync("hello");
                }
            }
        }
        #endregion

        return service;
    }

    private static async Task CreateHttpService3()
    {
        var config = new TouchSocketConfig();
        config.SetListenIPHosts(7789)
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();
             })
             .ConfigurePlugins(a =>
             {
                 //添加WebSocket功能
                 a.UseWebSocket(options =>
                 {
                     options.SetAutoPong(true);//当收到Ping报文时自动回应Pong

                     #region WebSocket连接验证
                     //添加自定义验证逻辑后，必须自己验证Url
                     options.SetVerifyConnection(async (client, context) =>
                     {
                         if (!context.Request.IsUpgrade())
                         {
                             //如果不包含升级协议的header，就直接返回false。
                             return false;
                         }
                         // 在此处添加验证逻辑，例如：匹配特定的URL
                         if (context.Request.UrlEquals("/ws"))
                         {
                             return true; // 允许连接
                         }

                         //如果url不匹配，则可以直接拒绝。
                         //await context.Response
                         //.SetStatus(403, "url不正确")
                         //.AnswerAsync();
                         return false;
                     });
                     #endregion
                 });
             });

        #region 使用WebSocket接收插件
        config.ConfigurePlugins(a =>
        {
            //添加WebSocket功能
            a.UseWebSocket("/ws");

            a.Add<MyWebSocketReceivePlugin>();
        });
        #endregion


    }
    private static async Task<HttpService> CreateHttpService()
    {
        var service = new HttpService();

        var config = new TouchSocketConfig();
        await service.SetupAsync(new TouchSocketConfig()//加载配置
             .SetListenIPHosts(7789)
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();
                 a.AddRpcStore(store =>
                 {
                     store.RegisterServer<MyServer>();
                 });
             })
             .ConfigurePlugins(a =>
             {
                 a.UseWebSocket(options =>
                 {
                     options.SetUrl("/ws");//设置url直接可以连接。
                     options.SetVerifyConnection(VerifyConnection);
                     options.SetAutoPong(true);//当收到ping报文时自动回应pong
                 });

                 //a.Add<MyReadTextWebSocketPlugin>();

                 //a.Add<MyWSCommandLinePlugin>();
                 a.Add<MyWebSocketPlugin>();

                 a.UseWebApi();
             }));
        await service.StartAsync();

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
    private static async Task<bool> VerifyConnection(IHttpSessionClient client, HttpContext context)
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
                await context.Response
                     .SetStatus(403, "token不正确")
                     .AnswerAsync();
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
                await context.Response
                      .SetStatus(403, "token不正确")
                      .AnswerAsync();
            }
        }
        return false;
    }


    #region WebSocket服务器使用ReadAsync读取数据
    internal class MyReadTextWebSocketPlugin : PluginBase, IWebSocketHandshakedPlugin
    {
        public async Task OnWebSocketHandshaked(IWebSocket client, HttpContextEventArgs e)
        {
            //当WebSocket想要使用ReadAsync时，需要设置此值为true
            client.AllowAsyncRead = true;

            while (true)
            {
                using (var receiveResult = await client.ReadAsync(CancellationToken.None))
                {
                    if (receiveResult.IsCompleted)
                    {
                        Console.WriteLine($"WebSocket连接已关闭，关闭代码：{client.CloseStatus}，信息：{receiveResult.Message}");
                        break;
                    }

                    //处理接收的数据
                    var dataFrame = receiveResult.DataFrame;
                    switch (dataFrame.Opcode)
                    {
                        case WSDataType.Cont:
                            //处理中继包
                            break;
                        case WSDataType.Text:
                            //处理文本包
                            Console.WriteLine(dataFrame.ToText());
                            break;
                        case WSDataType.Binary:
                            //处理二进制包
                            var data = dataFrame.PayloadData;
                            Console.WriteLine($"收到二进制数据，长度：{data.Length}");
                            break;
                        case WSDataType.Close:
                            //处理关闭包
                            break;
                        case WSDataType.Ping:
                            //处理Ping包
                            break;
                        case WSDataType.Pong:
                            //处理Pong包
                            break;
                        default:
                            //处理其他包
                            break;
                    }
                }
            }

            await e.InvokeNext();
        }
    }
    #endregion


    public class MyWebSocketPlugin : PluginBase,
        IWebSocketHandshakingPlugin,
        IWebSocketHandshakedPlugin,
        IWebSocketReceivedPlugin,
        IWebSocketClosingPlugin,
        IWebSocketClosedPlugin
    {
        public MyWebSocketPlugin(ILog logger)
        {
            this.m_logger = logger;
        }

        public async Task OnWebSocketHandshaking(IWebSocket client, HttpContextEventArgs e)
        {
            if (client.Client is IHttpSessionClient socketClient)
            {
                //服务端

                var id = socketClient.Id;
            }
            else if (client.Client is IHttpClient httpClient)
            {
                //客户端
            }
            this.m_logger.Info("WebSocket正在连接");
            await e.InvokeNext();
        }

        public async Task OnWebSocketHandshaked(IWebSocket client, HttpContextEventArgs e)
        {
            this.m_logger.Info("WebSocket成功连接");
            await e.InvokeNext();
        }

        private readonly ILog m_logger;

        #region WebSocket服务器处理中继数据
        public async Task OnWebSocketReceived(IWebSocket client, WSDataFrameEventArgs e)
        {
            var dataFrame = e.DataFrame;
            switch (dataFrame.Opcode)
            {
                case WSDataType.Close:
                    {
                        this.m_logger.Info("远程请求断开");
                        await client.CloseAsync("断开");
                    }
                    return;

                case WSDataType.Ping:
                    this.m_logger.Info("Ping");
                    await client.PongAsync();//收到ping时，一般需要响应pong
                    break;

                case WSDataType.Pong:
                    this.m_logger.Info("Pong");
                    break;

                default:
                    {

                        //其他报文，需要考虑中继包的情况。所以需要手动合并 WSDataType.Cont类型的包。
                        //或者使用消息合并器

                        //获取消息组合器
                        var messageCombinator = client.GetMessageCombinator();

                        try
                        {
                            //尝试组合
                            if (messageCombinator.TryCombine(e.DataFrame, out var webSocketMessage))
                            {
                                //组合成功，必须using释放模式
                                using (webSocketMessage)
                                {
                                    //合并后的消息
                                    var dataType = webSocketMessage.Opcode;

                                    //合并后的完整消息
                                    var data = webSocketMessage.PayloadData;

                                    if (dataType == WSDataType.Text)
                                    {
                                        this.m_logger.Info($"{dataType}|{data.Span.ToString(Encoding.UTF8)}");
                                        //按文本处理
                                    }
                                    else if (dataType == WSDataType.Binary)
                                    {
                                        //按字节处理
                                    }
                                    else
                                    {
                                        //可能是其他自定义协议
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.m_logger.Exception(ex);
                            messageCombinator.Clear();//当组合发生异常时，应该清空组合器数据
                        }

                    }
                    break;
            }

            await e.InvokeNext();
        }
        #endregion


        public async Task OnWebSocketClosed(IWebSocket webSocket, ClosedEventArgs e)
        {
            this.m_logger.Info($"WebSocket已断开，状态：{webSocket.CloseStatus}，信息：{e.Message}");
            await e.InvokeNext();
        }

        public async Task OnWebSocketClosing(IWebSocket webSocket, ClosingEventArgs e)
        {
            this.m_logger.Info($"WebSocket请求断开，状态：{webSocket.CloseStatus}，信息：{e.Message}");
            await e.InvokeNext();
        }
    }

    #region 创建WebSocket接收插件
    class MyWebSocketReceivePlugin : PluginBase, IWebSocketReceivedPlugin
    {
        public async Task OnWebSocketReceived(IWebSocket webSocket, WSDataFrameEventArgs e)
        {
            //处理接收的数据
            var dataFrame = e.DataFrame;
            switch (dataFrame.Opcode)
            {
                case WSDataType.Cont:
                    //处理中继包
                    break;
                case WSDataType.Text:
                    //处理文本包
                    Console.WriteLine(dataFrame.ToText());
                    break;
                case WSDataType.Binary:
                    //处理二进制包
                    var data = dataFrame.PayloadData;
                    Console.WriteLine($"收到二进制数据，长度：{data.Length}");
                    break;
                case WSDataType.Close:
                    //处理关闭包
                    break;
                case WSDataType.Ping:
                    //处理Ping包
                    break;
                case WSDataType.Pong:
                    //处理Pong包
                    break;
                default:
                    //处理其他包
                    break;
            }
            await e.InvokeNext();
        }
    }
    #endregion


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

        //当第一个参数，直接或间接实现ITcpSession接口时，会收集到当前请求的客户端，从而可以获取IP等。
        public SumClass SumCommand(IHttpSession client, SumClass sumClass)
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

    public class MyServer : SingletonRpcServer
    {
        private readonly ILog m_logger;

        public MyServer(ILog logger)
        {
            this.m_logger = logger;
        }

        [Router("/[api]/ws")]
        [Router("/[api]/[action]")]
        [WebApi(Method = HttpMethodType.Get)]
        public async Task ConnectWS(IWebApiCallContext callContext)
        {
            if (callContext.Caller is HttpSessionClient socketClient)
            {
                var result = await socketClient.SwitchProtocolToWebSocketAsync(callContext.HttpContext);
                if (!result.IsSuccess)
                {
                    this.m_logger.Error(result.Message);
                    return;
                }

                this.m_logger.Info("WS通过WebApi连接");
            }
        }
    }

    #region 自定义转换WebSocket
    class CustomWebSocketPlugin : PluginBase, IHttpPlugin
    {
        public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
        {
            if (e.Context.Request.IsUpgrade())
            {
                if (e.Context.Request.UrlEquals("/customws"))
                {
                    var result = await client.SwitchProtocolToWebSocketAsync(e.Context);
                    if (!result.IsSuccess)
                    {
                        //切换协议失败
                        return;
                    }
                    //切换协议成功，下面可以使用wsClient进行收发数据了。
                    var webSocket = client.WebSocket;

                }
            }
            await e.InvokeNext();
        }
    }
    #endregion

}

