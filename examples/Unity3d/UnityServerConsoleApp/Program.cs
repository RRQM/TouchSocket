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
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace UnityServerConsoleApp
{
    internal class Program
    {
        static UdpSession udpService = new UdpSession();
        static HttpDmtpService httpDmtpService = new HttpDmtpService();
        static TcpService tcpService = new TcpService();

        private static async Task Main(string[] args)
        {
            //unitypackage在本级目录下。
            await StartTcpService(7789);
            await StartDmtpService(7790);
            await StartUdpService(7791);
            Console.ReadKey();
        }


        private static async Task StartUdpService(int port)
        {

            udpService.Received = async (c, e) =>
            {
                await udpService.SendAsync(e.EndPoint, e.ByteBlock.Memory);
                Console.WriteLine($"收到：{e.ByteBlock.Span.ToString(Encoding.UTF8)}");
            };
            await udpService.SetupAsync(new TouchSocketConfig()
                   .SetBindIPHost(new IPHost(port))
                   .SetUdpDataHandlingAdapter(() => new NormalUdpDataHandlingAdapter())//常规udp
                                                                                       //.SetUdpDataHandlingAdapter(() => new UdpPackageAdapter())//Udp包模式，支持超过64k数据。
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();//添加一个日志注入
                   }));
            await udpService.StartAsync();

            udpService.Logger.Info($"UdpService已启动，端口：{port}");
        }


        private static async Task StartDmtpService(int port)
        {

            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(port)
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();//注册一个日志组

                       //注册rpc服务
                       a.AddRpcStore(store =>
                       {
                           store.RegisterServer<MyRpcServer>();
#if DEBUG
                           var code = store.GetProxyCodes("UnityRpcProxy", typeof(DmtpRpcAttribute), typeof(JsonRpcAttribute));
                           File.WriteAllText("../../../UnityRpcProxy.cs", code);
#endif
                       });
                   })
                   .ConfigurePlugins(a =>
                   {
                       //启用dmtp rpc插件
                       a.UseDmtpRpc();

                       //启用websocket插件
                       a.UseWebSocket()
                       .SetWSUrl("/ws");

                       //启用json rpc插件
                       a.UseWebSocketJsonRpc()
                       .SetAllowJsonRpc((websocket, context) => true);//让所有请求WebSocket都加载JsonRpc插件

                       a.Add<MyTcpRpcPlguin>();
                   })
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "Dmtp"//设置验证token
                   });

            await httpDmtpService.SetupAsync(config);
            await httpDmtpService.StartAsync();

            httpDmtpService.Logger.Info($"{httpDmtpService.GetType().Name}已启动，监听端口：{port}");
        }

        private static async Task StartTcpService(int port)
        {

            await tcpService.SetupAsync(new TouchSocketConfig()//载入配置
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
            await tcpService.StartAsync();//启动
            tcpService.Logger.Info($"Tcp服务器已启动，端口{port}");
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
        [DmtpRpc(MethodInvoke = true, MethodName = "DmtpRpc_{0}")]
        [JsonRpc(MethodInvoke = true, MethodName = "JsonRpc_{0}")]
        public MyLoginModelResult Login(ICallContext callContext, MyLoginModel model)
        {
            if (model.Account == "123" && model.Password == "abc")
            {
                return new MyLoginModelResult() { ResultCode = ResultCode.Success, Message = "Success" };
            }

            return new MyLoginModelResult() { ResultCode = ResultCode.Fail, Message = "账号或密码错误" };
        }

        [Description("性能测试")]
        [DmtpRpc(MethodInvoke = true, MethodName = "DmtpRpc_{0}")]
        [JsonRpc(MethodInvoke = true, MethodName = "JsonRpc_{0}")]
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
        public ResultCode ResultCode { get; set; }
        public string Message { get; set; }
    }

}