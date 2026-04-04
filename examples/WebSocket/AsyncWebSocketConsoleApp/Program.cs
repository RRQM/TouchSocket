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

using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;

namespace SyncWebSocketConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var service = await CreateHttpService();

        #region WebSocket客户端使用ReadAsync读取数据
        using (var client = await GetClient())
        {
            while (true)
            {
                //发送数据
                await client.SendAsync(Console.ReadLine());

                //设置1分钟的接收超时
                using var cts = new CancellationTokenSource(1000 * 60);
                using (var receiveResult = await client.ReadAsync(cts.Token))
                {
                    if (receiveResult.IsCompleted)
                    {
                        //但对于客户端来说，可以直接使用client.Online来判断连接状态。
                        Console.WriteLine($"WebSocket连接已关闭，关闭代码：{client.CloseStatus}，信息：{receiveResult.Message}");
                        break;
                    }

                    var dataFrame = receiveResult.DataFrame;

                    client.Logger.Info($"客户端收到数据：{dataFrame.ToText()}");
                }
            }
        }
        #endregion

    }

    /// <summary>
    /// 通过/ws直接连接
    /// </summary>
    private static async Task<IWebSocket> GetClient()
    {
        var client = new AsyncReadWebSocketClient();
        await client.SetupAsync(new TouchSocketConfig()
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();
             })
        #region WebSocket断线重连
             .ConfigurePlugins(a =>
             {
                 a.UseReconnection<WebSocketClient>(options =>
                 {
                     //设置在线状态轮询时间为5秒钟。
                     options.PollingInterval = TimeSpan.FromSeconds(5);

                     //使用websocket专门的心跳在线检测。基本逻辑如下：
                     //由于设置的PollingInterval为5秒，所以每5秒会检查一下WebSocketClient在线状态。
                     //1.每隔pingInterval的时间，发送一次ping报文。
                     //2.如果在activeTimeSpan（30秒）内，有数据收发，则也会认为在活。
                     options.UseWebSocketCheckAction(activeTimeSpan: TimeSpan.FromSeconds(30), pingInterval: TimeSpan.FromSeconds(5));
                 });
             })
        #endregion

             .SetRemoteIPHost("ws://127.0.0.1:7789/ws"));
        await client.ConnectAsync();

        client.Logger.Info("通过ws://127.0.0.1:7789/ws连接成功");
        return client;
    }

    private static async Task<HttpService> CreateHttpService()
    {
        var service = new HttpService();
        await service.SetupAsync(new TouchSocketConfig()//加载配置
             .SetListenIPHosts(7789)
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();
             })
             .ConfigurePlugins(a =>
             {
                 ////添加WebSocket功能
                 //a.UseWebSocket(options =>
                 //{
                 //    options.SetUrl("/ws");//设置url直接可以连接。
                 //    options.SetAutoPong(true);//当收到ping报文时自动回应pong
                 //});

                 a.Add<MyReadWebSocketPlugin>();
             }));
        await service.StartAsync();

        service.Logger.Info("服务器已启动");
        service.Logger.Info("直接连接地址=>ws://127.0.0.1:7789/ws");
        return service;
    }
}

#region WebSocket创建AsyncRead客户端
/// <summary>
/// 当WebSocket客户端想要使用ReadAsync来读取数据时，需通过继承<see cref="WebSocketClientBase"/>自行完成连接。
/// </summary>
class AsyncReadWebSocketClient : WebSocketClientBase, IConnectableClient
{
    private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        await this.semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            //当WebSocket想要使用ReadAsync时，需要设置此值为false，意为不进行自动接收数据。
            //autoReceive=false 表示不启用框架的自动接收循环，从而由调用者通过ReadAsync主动拉取数据。
            await base.ProtectedWebSocketConnectAsync(false, cancellationToken);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }
}
#endregion

#region WebSocket服务器使用ReadAsync读取数据
internal class MyReadWebSocketPlugin : PluginBase,IHttpPlugin, IWebSocketConnectedPlugin
{
    private readonly ILog m_logger;

    public MyReadWebSocketPlugin(ILog logger)
    {
        this.m_logger = logger;
    }

    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        //1.如果想要自行从websocket读取数据，则需要自己负责连接判断，不能通过a.UseWebSocket()插件。
        if (e.Context.Request.UrlEquals("/ws"))
        {
            //2.如果是连接到/ws，则升级为WebSocket连接。
            //当WebSocket想要使用ReadAsync时，需要设置此值为false，意为不进行自动接收数据。
            var result=await client.SwitchProtocolToWebSocketAsync(false);
            if (!result.IsSuccess)
            {
                client.Logger.Error($"升级WebSocket失败，信息：{result.Message}");
                return;
            }

            var webSocket = client.WebSocket;
            var cancellationToken = client.ClosedToken;

            //3.启动一个新的任务来读取WebSocket数据。不能在当前任务线程直接读取，因为只有当前任务返回后，websocket才算是完成升级。
            //或者可以直接返回，在OnWebSocketConnected事件中启动异步读取任务也是一样的。
            _ = EasyTask.SafeNewRun( () => StartWebSocketRead(webSocket,cancellationToken));
            return;
        }
        await e.InvokeNext();
    }

    private async Task StartWebSocketRead(IWebSocket webSocket,CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            //设置1分钟的接收超时
            using var cts = new CancellationTokenSource(1000 * 60);
            using (var receiveResult = await webSocket.ReadAsync(cts.Token))
            {
                if (receiveResult.IsCompleted)
                {
                    Console.WriteLine($"WebSocket连接已关闭，关闭代码：{webSocket.CloseStatus}，信息：{receiveResult.Message}");
                    break;
                }

                var dataFrame = receiveResult.DataFrame;

                this.m_logger.Info($"服务器收到数据：{dataFrame.ToText()}");

                //回应客户端
                await webSocket.SendAsync(dataFrame.ToText());
            }

            //此处即表明websocket已断开连接
            this.m_logger.Info("WebSocket断开连接");
        }
    }

    public async Task OnWebSocketConnected(IWebSocket client, HttpContextEventArgs e)
    {
        var webSocket = client;
        var cancellationToken = client.ClosedToken;

        //4.当WebSocket连接成功后，触发此事件。
        //如果在自行连接时没有处理接收，则可以在此事件中启动异步读取任务也是一样的。
        await e.InvokeNext();
    }
}
#endregion

/// <summary>
/// 演示使用扩展方法读取字符串/二进制数据
/// </summary>
internal static class ReadExtensionDemos
{
    #region WebSocket使用ReadStringAsync读取字符串
    public static async Task ReadStringDemo(IWebSocket webSocket)
    {
        //ReadStringAsync 是对 ReadAsync 的封装，自动处理分包、拼包，直接返回完整字符串。
        //收到非字符串类型的帧时会抛出异常。
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
        var text = await webSocket.ReadStringAsync(cts.Token);
        Console.WriteLine($"收到字符串：{text}");
    }
    #endregion

    #region WebSocket使用ReadBinaryAsync读取二进制
    public static async Task ReadBinaryDemo(IWebSocket webSocket)
    {
        //ReadBinaryAsync 是对 ReadAsync 的封装，自动处理分包、拼包，直接将完整二进制数据写入 ByteBlock。
        //收到非二进制类型的帧时会抛出异常。
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
        using var byteBlock = new ByteBlock(1024 * 64);
        await webSocket.ReadBinaryAsync(byteBlock, cts.Token);
        Console.WriteLine($"收到二进制数据，长度：{byteBlock.Length}");
    }
    #endregion
}
