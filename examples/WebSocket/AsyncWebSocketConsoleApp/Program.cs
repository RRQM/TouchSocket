using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;

namespace SyncWebSocketConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var service = CreateHttpService();
            using (var client = GetClient())
            {
                //通过GetWebSocket扩展方法，获取到显式的WebSocket终端。
                //需要注意的是，此处的using，仅仅是释放当前创建的WebSocket。而不是WebSocketClient的连接。
                using (var websocket=client.GetWebSocket())
                {
                    while (true)
                    {
                        websocket.Send(Console.ReadLine());
                    }
                }
            }
        }

        /// <summary>
        /// 通过/ws直接连接
        /// </summary>
        private static WebSocketClient GetClient()
        {
            var client = new WebSocketClient();
            client.Setup(new TouchSocketConfig()
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .SetRemoteIPHost("ws://127.0.0.1:7789/ws"));
            client.Connect();

            client.Logger.Info("通过ws://127.0.0.1:7789/ws连接成功");
            return client;
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
                      .SetWSUrl("/ws")//设置url直接可以连接。
                      .UseAutoPong();//当收到ping报文时自动回应pong

                    a.Add<MyReadWebSocketPlugin>();
                }));
            service.Start();

            service.Logger.Info("服务器已启动");
            service.Logger.Info("直接连接地址=>ws://127.0.0.1:7789/ws");
            return service;
        }
    }

    class MyReadWebSocketPlugin : PluginBase, IWebSocketHandshakedPlugin
    {
        private readonly ILog m_logger;

        public MyReadWebSocketPlugin(ILog logger)
        {
            this.m_logger = logger;
        }
        public async Task OnWebSocketHandshaked(IHttpClientBase client, HttpContextEventArgs e)
        {
            using (var websocket = client.GetWebSocket())
            {
                //此处即表明websocket已连接
                while (true)
                {
                    using (var receiveResult=await websocket.ReadAsync(CancellationToken.None))
                    {
                        
                        if (receiveResult.DataFrame==null)
                        {
                            break;
                        }

                        //判断是否为最后数据
                        //例如发送方发送了一个10Mb的数据，接收时可能会多次接收，所以需要此属性判断。
                        if (receiveResult.DataFrame.FIN)
                        {
                            if (receiveResult.DataFrame.IsText)
                            {
                                m_logger.Info($"WebSocket文本：{receiveResult.DataFrame.ToText()}");
                            }
                        }
                        
                    }
                }

                //此处即表明websocket已断开连接
                m_logger.Info("WebSocket断开连接");
            }
            await e.InvokeNext();
        }
    }
}