using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace UnityServerConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //unitypackage在本级目录下。
            //已发布的apk和exe客户端也在本级目录下。
            StartTcpService(7789);
            StartTcpRpcService(7790);
            StartUdpService(7791);
            Console.ReadKey();
        }

        private static void StartUdpService(int port)
        {
            var udpService = new UdpSession();
            udpService.Received = (c, e) =>
            {
                udpService.Send(e.EndPoint, e.ByteBlock);
                Console.WriteLine($"收到：{Encoding.UTF8.GetString(e.ByteBlock.Buffer, 0, e.ByteBlock.Len)}");
                return EasyTask.CompletedTask;
            };
            udpService.Setup(new TouchSocketConfig()
                 .SetBindIPHost(new IPHost(port))
                 .SetUdpDataHandlingAdapter(() => new NormalUdpDataHandlingAdapter())//常规udp
                                                                                     //.SetUdpDataHandlingAdapter(() => new UdpPackageAdapter())//Udp包模式，支持超过64k数据。
                 .ConfigureContainer(a =>
                 {
                     a.AddConsoleLogger();//添加一个日志注入
                 }));
            udpService.Start();

            udpService.Logger.Info($"UdpService已启动，端口：{port}");
        }

        private static void StartTcpRpcService(int port)
        {
            var service = new TcpDmtpService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(port) })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();//注册一个日志组
                       a.AddRpcStore(store =>
                       {
                           store.RegisterServer<MyRpcServer>();
#if DEBUG
                           var code = store.GetProxyCodes("UnityRpcProxy", typeof(DmtpRpcAttribute));
                           File.WriteAllText("../../../UnityRpcProxy.cs", code);
#endif
                       });
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRpc();

                       a.Add<MyTcpRpcPlguin>();
                   })
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "Dmtp"
                   });

            service.Setup(config);
            service.Start();

            service.Logger.Info($"{service.GetType().Name}已启动，监听端口：{port}");
        }

        private static void StartTcpService(int port)
        {
            var service = new TcpService();
            service.Setup(new TouchSocketConfig()//载入配置
                .SetListenIPHosts(new IPHost[] { new IPHost(port) })
                .SetTcpDataHandlingAdapter(() => new FixedHeaderPackageAdapter())
                .ConfigurePlugins(a =>
                {
                    a.Add<MyPlguin>();//此处可以添加插件
                })
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();//添加一个日志注入
                }));
            service.Start();//启动
            service.Logger.Info($"Tcp服务器已启动，端口{port}");
        }
    }

    internal class MyTcpRpcPlguin : PluginBase
    {
    }

    internal class MyPlguin : PluginBase, ITcpConnectedPlugin<ISocketClient>, ITcpDisconnectedPlugin<ISocketClient>, ITcpReceivedPlugin<ISocketClient>
    {
        public async Task OnTcpConnected(ISocketClient client, ConnectedEventArgs e)
        {
            client.Logger.Info($"客户端{client.GetInfo()}已连接");
            await e.InvokeNext();
        }

        public async Task OnTcpDisconnected(ISocketClient client, DisconnectEventArgs e)
        {
            client.Logger.Info($"客户端{client.GetInfo()}已断开连接");
            await e.InvokeNext();
        }

        public async Task OnTcpReceived(ISocketClient client, ReceivedDataEventArgs e)
        {
            client.Logger.Info($"接收到信息：{Encoding.UTF8.GetString(e.ByteBlock.Buffer, 0, e.ByteBlock.Len)}");
            client.Send($"服务器已收到你发送的消息：{e.ByteBlock.ToString()}");
            await e.InvokeNext();
        }
    }

    /// <summary>
    /// 单例服务
    /// </summary>
    public partial class MyRpcServer : RpcServer
    {
        public MyRpcServer(ILog logger)
        {
            this.m_timer = new Timer((obj) =>
            {
                logger.Info($"count={this.count}");
            }, null, 0, 1000);
            this.m_logger = logger;
        }

        private Timer m_timer;
        private int count;
        private readonly ILog m_logger;

        [Description("登录")]
        [DmtpRpc(true)]
        public MyLoginModelResult Login(ICallContext callContext, MyLoginModel model)
        {
            if (model.Account == "123" && model.Password == "abc")
            {
                return new MyLoginModelResult() { Status = 1, Message = "Success" };
            }

            return new MyLoginModelResult() { Status = 2, Message = "账号或密码错误" };
        }

        [Description("性能测试")]
        [DmtpRpc(true)]
        public int Performance(int i)
        {
            Interlocked.Increment(ref this.count);
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