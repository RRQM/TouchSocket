using System.ComponentModel;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Core.Plugins;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.Sockets.Plugins;
using TouchSocket.Rpc.TouchRpc.Plugins;
using System;
using System.IO;
using System.Threading;

namespace UnityServerConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //unitypackage在本级目录下。
            //已发布的apk和exe客户端也在本级目录下。
            StartTcpService();
            StartTcpRpcService();
            Console.ReadKey();
        }

        static void StartTcpRpcService()
        {
            var service = new TcpTouchRpcService();
            TouchSocketConfig config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(7790) })
                   .SetThreadCount(50)
                   .UseDelaySender()
                   .UsePlugin()
                   .ConfigureContainer(a =>
                   {
                       a.SetLogger<ConsoleLogger>();//注册一个日志组
                   })
                   .ConfigureRpcStore(a =>
                   {
                       a.RegisterServer<MyRpcServer>();//注册服务
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.Add<MyTcpRpcPlguin>();
                   })
                   .SetVerifyToken("TouchRpc");

            service.Setup(config)
                .Start();

            service.Logger.Info($"{service.GetType().Name}已启动，监听端口：{7790}");

            string code = service.RpcStore.GetProxyCodes("TcpRpcProxy");
            File.WriteAllText("TcpRpcProxy.cs", code);

            //service.RpcStore.ShareProxy(new IPHost(8848));
        }

        static void StartTcpService()
        {
            TcpService service = new TcpService();
            service.Setup(new TouchSocketConfig()//载入配置     
                .SetListenIPHosts(new IPHost[] { new IPHost(7789) })//同时监听两个地址
                .SetMaxCount(10000)
                .SetThreadCount(10)
                .UsePlugin()
                .SetDataHandlingAdapter(() => new FixedHeaderPackageAdapter())
                .ConfigurePlugins(a =>
                {
                    a.Add<MyPlguin>();//此处可以添加插件
                })
                .ConfigureContainer(a =>
                {
                    a.SetSingletonLogger<ConsoleLogger>();//添加一个日志注入
                }))
                .Start();//启动
            service.Logger.Info("Tcp服务器已启动，端口7789");
        }
    }

    class MyTcpRpcPlguin : TouchRpcPluginBase<TcpTouchRpcSocketClient>
    {
        protected override void OnStreamTransfering(TcpTouchRpcSocketClient client, StreamOperationEventArgs e)
        {
            client.Logger.Info($"客户端：{client.GetInfo()}正在传输流....，总长度={e.StreamInfo.Size}");
            e.Bucket = new MemoryStream();
        }

        protected override void OnStreamTransfered(TcpTouchRpcSocketClient client, StreamStatusEventArgs e)
        {
            client.Logger.Info($"客户端：{client.GetInfo()}流传输结束，状态={e.Result}");
            e.Bucket.SafeDispose();
        }
    }

    class MyPlguin : TcpPluginBase<SocketClient>
    {
        protected override void OnConnected(SocketClient client, TouchSocketEventArgs e)
        {
            client.Logger.Info($"客户端{client.GetInfo()}已连接");
        }
        protected override void OnDisconnected(SocketClient client, ClientDisconnectedEventArgs e)
        {
            client.Logger.Info($"客户端{client.GetInfo()}已断开连接");
        }

        protected override void OnReceivedData(SocketClient client, ReceivedDataEventArgs e)
        {
            client.Logger.Info($"接收到信息：{Encoding.UTF8.GetString(e.ByteBlock.Buffer, 0, e.ByteBlock.Len)}");
            client.Send($"服务器已收到你发送的消息：{e.ByteBlock.ToString()}");
        }
    }

    /// <summary>
    /// 单例服务
    /// </summary>
    public class MyRpcServer : RpcServer
    {
        public MyRpcServer(ILog logger)
        {
            this.m_timer = new Timer((obj) =>
            {
                logger.Info($"count={count}");
            }, null, 0, 1000);
            this.m_logger = logger;
        }

        Timer m_timer;
        int count;
        private readonly ILog m_logger;

        [Description("登录")]
        [TouchRpc(true, MethodFlags = MethodFlags.IncludeCallContext)]
        public MyLoginModelResult Login(ICallContext callContext, MyLoginModel model)
        {
            if (model.Account == "123" && model.Password == "abc")
            {
                return new MyLoginModelResult() { Status = 1, Message = "Success" };
            }

            return new MyLoginModelResult() { Status = 2, Message = "账号或密码错误" };
        }

        [Description("性能测试")]
        [TouchRpc(true)]
        public int Performance(int i)
        {
            Interlocked.Increment(ref count);
            return ++i;
        }
    }
    public class MyLoginModel
    {
        public string Token { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
    }
    public class MyLoginModelResult
    {
        public byte Status { get; set; }
        public string Message { get; set; }
    }
}