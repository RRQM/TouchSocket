using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Rpc.WebApi;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace WSVerifyConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WebApiVerify();
            Console.ReadKey();
        }

        static void WebApiVerify()
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
                    a.UseWebApi();
                    a.UseWebSocket();//不用设置连接url
                }))
                .Start();

            Console.WriteLine("服务器已启动，可使用下列地址连接");
            Console.WriteLine("ws://127.0.0.1:7789/MyServer/ConnectWS?token=123");

            WebSocketClient myWSClient = new WebSocketClient();
            myWSClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("ws://127.0.0.1:7789/MyServer/ConnectWS?token=1234")
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                }));
            var result = myWSClient.TryConnect();

            myWSClient.Logger.Info(result.ToString());
        }

        static void NormalVerify()
        {
            var service = new HttpService();
            service.Setup(new TouchSocketConfig()//加载配置
                .UsePlugin()
                .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.UseWebSocket()//添加WebSocket功能
                           .SetWSUrl("/ws");
                    //.SetCallback(WSCallback);//WSCallback回调函数是在WS收到数据时触发回调的。下面会用插件，所以我们不使用这种方式
                    a.Add<MyWebSocketPlugin>();//MyWebSocketPlugin是继承自WebSocketPluginBase的插件。
                }))
                .Start();

            service.Logger.Info("Http服务器已启动");
            service.Logger.Info("连接url：ws://127.0.0.1:7789/ws?token=123");


            WebSocketClient myWSClient = new WebSocketClient();
            myWSClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("ws://127.0.0.1:7789/ws?token=123")
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                }));
            var result = myWSClient.TryConnect();

            myWSClient.Logger.Info(result.ToString());
        }
    }

    public class MyServer : RpcServer
    {
        private readonly ILog m_logger;

        public MyServer(ILog logger)
        {
            this.m_logger = logger;
        }

        [WebApi(HttpMethodType.GET, MethodFlags = MethodFlags.IncludeCallContext)]
        public void ConnectWS(IWebApiCallContext callContext, string token)
        {
            if (token != "123")
            {
                callContext.HttpContext.Response
                    .SetStatus("400", "口令不正确")
                    .Answer();
                return;
            }
            //下面进行连接
            if (callContext.Caller is HttpSocketClient socketClient)
            {
                if (socketClient.SwitchProtocolToWebSocket(callContext.HttpContext))
                {
                    m_logger.Info("WS通过WebApi连接");
                }
            }
        }
    }

    class MyWebSocketPlugin : WebSocketPluginBase<HttpSocketClient>
    {
        protected override void OnHandshaking(HttpSocketClient client, HttpContextEventArgs e)
        {
            if (e.Context.Request.Query["token"] != "123")
            {
                e.IsPermitOperation = false; //参数不符合，直接拒绝
                e.Handled = true;//表示该条消息已被本插件处理，不需要在向其他插件投递了。
                return;

                //或者直接回复，此处部分和http操作一致。
                e.Context.Response
                    .SetStatus("400", "口令不正确")
                    .Answer();

            }
            base.OnHandshaking(client, e);
        }

        protected override void OnHandleWSDataFrame(HttpSocketClient client, WSDataFrameEventArgs e)
        {
            if (e.DataFrame.Opcode == WSDataType.Text)//文本数据
            {
                client.Logger.Info($"收到信息：{e.DataFrame.ToText()}");
            }
            else if (e.DataFrame.Opcode == WSDataType.Binary)//二进制
            {
                byte[] data = e.DataFrame.PayloadData.ToArray();
            }
        }
    }
}