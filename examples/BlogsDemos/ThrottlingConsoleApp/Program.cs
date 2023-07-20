using System;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThrottlingConsoleApp
{
    internal class Program
    {
        /// <summary>
        /// 限制单个客户端的访问流量
        /// 博客连接<see href="https://blog.csdn.net/qq_40374647/article/details/125496769"/>
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            TcpService service = new TcpService();
            service.Received = (client, byteBlock, requestInfo) =>
            {
                //从客户端收到信息
                string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                client.Logger.Info($"已从{client.ID}接收到信息：{mes}");
            };

            service.Setup(new TouchSocketConfig()//载入配置
                .UsePlugin()
                .SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
                .ConfigureContainer(a => 
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a => 
                {
                    a.Add<MyThrottlingPlugin>();
                }))
                .Start();//启动
            service.Logger.Info("服务器已启动");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// 一个流量计数器扩展。
    /// </summary>
    internal static class DependencyExtensions
    {
        public static readonly DependencyProperty<FlowGate> FlowGateProperty =
            DependencyProperty<FlowGate>.Register("FlowGate", typeof(DependencyExtensions), null);

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

        protected override void OnReceivingData(ITcpClientBase client, ByteBlockEventArgs e)
        {
            client.GetFlowGate().AddCheckWait(e.ByteBlock.Len);
            base.OnReceivingData(client, e);
        }
    }
}