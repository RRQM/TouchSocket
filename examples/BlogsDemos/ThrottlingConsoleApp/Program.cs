using System;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Plugins;
using TouchSocket.Sockets;
using TouchSocket.Sockets.Plugins;

namespace ThrottlingConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TcpService service = new TcpService();
            service.Connecting += (client, e) => { };//有客户端正在连接
            service.Connected += (client, e) => { };//有客户端连接
            service.Disconnected += (client, e) => { };//有客户端断开连接
            service.Received += (client, byteBlock, requestInfo) =>
            {
                //从客户端收到信息
                string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                Console.WriteLine($"已从{client.ID}接收到信息：{mes}");

                client.Send(mes);//将收到的信息直接返回给发送方
            };

            service.Setup(new TouchSocketConfig()//载入配置
                .UsePlugin()
                .SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
                .SetMaxCount(10000)
                .SetThreadCount(100))
                .Start();//启动
            service.AddPlugin<MyThrottlingPlugin>();

            Console.ReadLine();
        }
    }

    /// <summary>
    /// 一个流量计数器扩展。
    /// </summary>
    internal static class DependencyExtensions
    {
        public static readonly DependencyProperty FlowGateProperty =
            DependencyProperty.Register("FlowGate", typeof(FlowGate), typeof(DependencyExtensions), null);

        public static void InitFlowGate(this IDependencyObject dependencyObject, int max)
        {
            dependencyObject.SetValue(FlowGateProperty, new FlowGate() { Maximum = max });
        }

        public static FlowGate GetFlowGate(this IDependencyObject dependencyObject)
        {
            return dependencyObject.GetValue<FlowGate>(FlowGateProperty);
        }
    }

    public class MyThrottlingPlugin : TcpPluginBase
    {
        private readonly int m_max;

        [DependencyInject(10)]
        public MyThrottlingPlugin(int max)
        {
            this.m_max = max;
            this.Order = int.MaxValue;//提升优先级
        }

        protected override void OnConnected(ITcpClientBase client, TouchSocketEventArgs e)
        {
            client.InitFlowGate(this.m_max);//初始化流量计数器。
            base.OnConnected(client, e);
        }

        protected override void OnReceivedData(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            client.GetFlowGate().AddCheckWait(e.ByteBlock.Len);//此处假设接收的是ByteBlock数据。如果是自定义适配器，按需增量即可。
            base.OnReceivedData(client, e);
        }
    }
}