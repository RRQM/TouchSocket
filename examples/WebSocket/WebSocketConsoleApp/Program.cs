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

        consoleAction.ShowAll();

        var service = await CreateHttpService();

        await consoleAction.RunCommandLineAsync();
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
                 a.UseWebSocketReconnection();
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
        var client = new WebSocketClient();
        await client.SetupAsync(new TouchSocketConfig()
              .ConfigureContainer(a =>
              {
                  a.AddConsoleLogger();
              })
              .SetRemoteIPHost("ws://127.0.0.1:7789/ws"));
        await client.ConnectAsync();
        await client.SendAsync("hello");
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

                 a.UseWebSocketReconnection();
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

    private static async Task<HttpService> CreateHttpService()
    {
        var service = new HttpService();

        TouchSocketConfig config=new TouchSocketConfig();
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
                 a.UseWebSocket()//添加WebSocket功能
                                 //.SetWSUrl("/ws")//设置url直接可以连接。
                        .SetVerifyConnection(VerifyConnection)
                        //.UseAutoPong()//当收到ping报文时自动回应pong
                        ;

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


    internal class MyReadTextWebSocketPlugin : PluginBase, IWebSocketHandshakedPlugin
    {
        private readonly ILog m_logger;

        public MyReadTextWebSocketPlugin(ILog logger)
        {
            this.m_logger = logger;
        }

        public async Task OnWebSocketHandshaked(IWebSocket client, HttpContextEventArgs e)
        {
            //当WebSocket想要使用ReadAsync时，需要设置此值为true
            client.AllowAsyncRead = true;

            //此处即表明websocket已连接

            MemoryStream stream = default;//中继包缓存
            var isText = false;//标识是否为文本
            while (true)
            {
                using (var receiveResult = await client.ReadAsync(CancellationToken.None))
                {
                    if (receiveResult.IsCompleted)
                    {
                        break;
                    }

                    var dataFrame = receiveResult.DataFrame;
                    var data = receiveResult.DataFrame.PayloadData;

                    switch (dataFrame.Opcode)
                    {
                        case WSDataType.Cont:
                            {
                                //如果是非net9.0即以上，即：NetFramework平台使用。原因是stream不支持span写入
                                //var segment = data.AsSegment();
                                //stream.Write(segment.Array, segment.Offset, segment.Count);

                                //如果是net9.0以上，直接写入span即可
                                stream.Write(data.Span);

                                //收到的是中继包
                                if (dataFrame.FIN)//判断是否为最终包
                                {
                                    //是

                                    if (isText)//判断是否为文本
                                    {
                                        this.m_logger.Info($"WebSocket文本：{Encoding.UTF8.GetString(stream.ToArray())}");
                                    }
                                    else
                                    {
                                        this.m_logger.Info($"WebSocket二进制：{stream.Length}长度");
                                    }
                                }
                                else
                                {
                                    //否，继续缓存
                                }
                            }
                            break;
                        case WSDataType.Text:
                            {
                                if (dataFrame.FIN)//判断是不是最后的包
                                {
                                    //是，则直接输出
                                    //说明上次并没有中继数据缓存，直接输出本次内容即可
                                    this.m_logger.Info($"WebSocket文本：{dataFrame.ToText()}");
                                }
                                else
                                {
                                    isText = true;

                                    //否，则说明数据太大了，分中继包了。
                                    //则，初始化缓存容器
                                    stream ??= new MemoryStream();

                                    //下面则是缓存逻辑

                                    //如果是非net9.0即以上，即：NetFramework平台使用。原因是stream不支持span写入
                                    //var segment = data.AsSegment();
                                    //stream.Write(segment.Array, segment.Offset, segment.Count);

                                    //如果是net9.0以上，直接写入span即可
                                    stream.Write(data.Span);
                                }
                            }
                            break;
                        case WSDataType.Binary:
                            {
                                if (dataFrame.FIN)//判断是不是最后的包
                                {
                                    //是，则直接输出
                                    //说明上次并没有中继数据缓存，直接输出本次内容即可
                                    this.m_logger.Info($"WebSocket二进制：{data.Length}长度");
                                }
                                else
                                {
                                    isText = false;

                                    //否，则说明数据太大了，分中继包了。
                                    //则，初始化缓存容器
                                    stream ??= new MemoryStream();

                                    //下面则是缓存逻辑

                                    //如果是非net9.0即以上，即：NetFramework平台使用。原因是stream不支持span写入
                                    //var segment = data.AsSegment();
                                    //stream.Write(segment.Array, segment.Offset, segment.Count);

                                    //如果是net9.0以上，直接写入span即可
                                    stream.Write(data.Span);
                                }
                            }
                            break;
                        case WSDataType.Close:
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

            //此处即表明websocket已断开连接
            this.m_logger.Info("WebSocket断开连接");
            await e.InvokeNext();
        }
    }

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

        public async Task OnWebSocketReceived(IWebSocket client, WSDataFrameEventArgs e)
        {
            switch (e.DataFrame.Opcode)
            {
                case WSDataType.Close:
                    {
                        this.m_logger.Info("远程请求断开");

                        //var byteBlock = e.DataFrame.PayloadData;
                        //byteBlock.SeekToStart();
                        //var ss = ReaderExtension.ReadValue<TReader,ushort>(ref byteBlockEndianType.Big);
                        //using (var frame = new WSDataFrame())
                        //{
                        //    frame.Opcode = WSDataType.Close;
                        //    frame.FIN = true;
                        //    frame.PayloadData=new ByteBlock(1024*64);
                        //    frame.PayloadData.WriteUInt16(1000, EndianType.Big);
                        //    frame.PayloadData.Write(Encoding.UTF8.GetBytes("hello"));
                        //    await client.SendAsync(frame);
                        //}

                        //client.Client.TryShutdown();

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

    private class MyHttpPlugin : PluginBase, IHttpPlugin
    {
        public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
        {
            if (e.Context.Request.IsGet())
            {
                if (e.Context.Request.UrlEquals("/GetSwitchToWebSocket"))
                {
                    var result = await client.SwitchProtocolToWebSocketAsync(e.Context);
                    return;
                }
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
}