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
            //当WebSocket想要使用ReadAsync时，需要设置此值为true
            client.AllowAsyncRead = true;
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
    private static async Task<WebSocketClient> GetClient()
    {
        var client = new WebSocketClient();
        await client.SetupAsync(new TouchSocketConfig()
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();
             })
        #region WebSocket断线重连
             .ConfigurePlugins(a =>
             {
                 a.UseReconnection<WebSocketClient>();

                 //使用健康插件进行绝对存活检测，默认10秒检测一次。
                 a.UseCheckClear<WebSocketClient>()
                 .SetTick(TimeSpan.FromSeconds(10));
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
                 //添加WebSocket功能
                 a.UseWebSocket(options =>
                 {
                     options.SetUrl("/ws");//设置url直接可以连接。
                     options.SetAutoPong(true);//当收到ping报文时自动回应pong
                 });

                 a.Add<MyReadWebSocketPlugin>();
             }));
        await service.StartAsync();

        service.Logger.Info("服务器已启动");
        service.Logger.Info("直接连接地址=>ws://127.0.0.1:7789/ws");
        return service;
    }
}

#region WebSocket服务器使用ReadAsync读取数据
internal class MyReadWebSocketPlugin : PluginBase, IWebSocketConnectedPlugin
{
    private readonly ILog m_logger;

    public MyReadWebSocketPlugin(ILog logger)
    {
        this.m_logger = logger;
    }

    public async Task OnWebSocketConnected(IWebSocket client, HttpContextEventArgs e)
    {
        //当WebSocket想要使用ReadAsync时，需要设置此值为true
        client.AllowAsyncRead = true;

        //此处即表明websocket已连接

        while (true)
        {
            //设置1分钟的接收超时
            using CancellationTokenSource cts = new CancellationTokenSource(1000*60);
            using (var receiveResult = await client.ReadAsync(cts.Token))
            {
                if (receiveResult.IsCompleted)
                {
                    Console.WriteLine($"WebSocket连接已关闭，关闭代码：{client.CloseStatus}，信息：{receiveResult.Message}");
                    break;
                }

                var dataFrame = receiveResult.DataFrame;

                this.m_logger.Info($"服务器收到数据：{dataFrame.ToText()}");

                //回应客户端
                await client.SendAsync(dataFrame.ToText());
            }
        }

        //此处即表明websocket已断开连接
        this.m_logger.Info("WebSocket断开连接");
        await e.InvokeNext();
    }
}
#endregion
