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
            StartTcpService(7789);
            StartTcpRpcService(7790);
            StartUdpService(7791);
            Console.ReadKey();
        }

        private static void StartUdpService(int port)
        {
            var udpService = new UdpSession();
            udpService.Received = async (c, e) =>
            {
                await udpService.SendAsync(e.EndPoint, e.ByteBlock.Memory);
                Console.WriteLine($"收到：{e.ByteBlock.Span.ToString(Encoding.UTF8)}");
            };
            udpService.SetupAsync(new TouchSocketConfig()
                 .SetBindIPHost(new IPHost(port))
                 .SetUdpDataHandlingAdapter(() => new NormalUdpDataHandlingAdapter())//常规udp
                                                                                     //.SetUdpDataHandlingAdapter(() => new UdpPackageAdapter())//Udp包模式，支持超过64k数据。
                 .ConfigureContainer(a =>
                 {
                     a.AddConsoleLogger();//添加一个日志注入
                 }));
            udpService.StartAsync();

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

            service.SetupAsync(config);
            service.StartAsync();

            service.Logger.Info($"{service.GetType().Name}已启动，监听端口：{port}");
        }

        private static void StartTcpService(int port)
        {
            var service = new TcpService();
            service.SetupAsync(new TouchSocketConfig()//载入配置
                .SetListenIPHosts(new IPHost(port))
                .SetTcpDataHandlingAdapter(() => new FixedHeaderPackageAdapter())
                .ConfigurePlugins(a =>
                {
                    a.Add<MyPlguin>();//此处可以添加插件
                })
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();//添加一个日志注入
                }));
            service.StartAsync();//启动
            service.Logger.Info($"Tcp服务器已启动，端口{port}");
        }
    }

    internal class MyTcpRpcPlguin : PluginBase
    {
    }

    internal class MyPlguin : PluginBase, ITcpConnectedPlugin, ITcpClosedPlugin, ITcpReceivedPlugin
    {
        public async Task OnTcpClosed(ITcpSession client, ClosedEventArgs e)
        {
            client.Logger.Info($"客户端{client.GetIPPort()}已断开");
            await e.InvokeNext();
        }

        public async Task OnTcpConnected(ITcpSession client, ConnectedEventArgs e)
        {
            client.Logger.Info($"客户端{client.GetIPPort()}已连接");
            await e.InvokeNext();
        }


        public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
        {
            client.Logger.Info($"接收到信息：{e.ByteBlock.Span.ToString(Encoding.UTF8)}");

            if (client is ITcpSessionClient sessionClient)
            {
                await sessionClient.SendAsync($"服务器已收到你发送的消息：{e.ByteBlock.ToString()}");
            }


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
        [DmtpRpc(MethodInvoke = true)]
        public MyLoginModelResult Login(ICallContext callContext, MyLoginModel model)
        {
            if (model.Account == "123" && model.Password == "abc")
            {
                return new MyLoginModelResult() { Status = 1, Message = "Success" };
            }

            return new MyLoginModelResult() { Status = 2, Message = "账号或密码错误" };
        }

        [Description("性能测试")]
        [DmtpRpc(MethodInvoke = true)]
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