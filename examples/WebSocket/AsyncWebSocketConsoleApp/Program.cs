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
        using (var client = await GetClient())
        {
            while (true)
            {
                await client.SendAsync(Console.ReadLine());
            }
        }
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

internal class MyReadWebSocketPlugin : PluginBase, IWebSocketHandshakedPlugin
{
    private readonly ILog m_logger;

    public MyReadWebSocketPlugin(ILog logger)
    {
        this.m_logger = logger;
    }

    public async Task OnWebSocketHandshaked(IWebSocket client, HttpContextEventArgs e)
    {
        //当WebSocket想要使用ReadAsync时，需要设置此值为true
        client.AllowAsyncRead = true;

        //此处即表明websocket已连接

        while (true)
        {
            using (var receiveResult = await client.ReadAsync(CancellationToken.None))
            {
                if (receiveResult.DataFrame == null)
                {
                    break;
                }

                //判断是否为最后数据
                //例如发送方发送了一个10Mb的数据，接收时可能会多次接收，所以需要此属性判断。
                if (receiveResult.DataFrame.FIN)
                {
                    if (receiveResult.DataFrame.IsText)
                    {
                        this.m_logger.Info($"WebSocket文本：{receiveResult.DataFrame.ToText()}");
                    }
                }
            }
        }

        //此处即表明websocket已断开连接
        this.m_logger.Info("WebSocket断开连接");
        await e.InvokeNext();
    }
}