using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace LimitNumberOfConnectionsConsoleApp
{
    internal class Program
    {
        /// <summary>
        /// 限制同一个IP的连接数量
        /// 博客地址<see href="https://blog.csdn.net/qq_40374647/article/details/125390655"/>
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            TcpService service = new TcpService();
            service.Setup(new TouchSocketConfig()//载入配置
                .SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<LimitNumberOfConnectionsPlugin>();
                })
                .UsePlugin())
                .Start();//启动
            service.Logger.Info("服务器已启动");
            Console.ReadKey();
        }
    }

    internal class Count
    {
        private int num;

        public int Num
        {
            get { return num; }
        }

        public int Decrement()
        {
            return Interlocked.Decrement(ref num);
        }

        public int Increment()
        {
            return Interlocked.Increment(ref num);
        }
    }

    internal class LimitNumberOfConnectionsPlugin : TcpPluginBase
    {
        private readonly ConcurrentDictionary<string, Count> m_ipToCount = new ConcurrentDictionary<string, Count>();

        private readonly ILog m_logger;

        [DependencyInject(2)]
        public LimitNumberOfConnectionsPlugin(int max, ILog logger)
        {
            this.Max = max;
            this.m_logger = logger;

            logger.Info($"限制连接插件生效，同一IP限制{max}个连接");
        }

        public LimitNumberOfConnectionsPlugin(ILog logger)
        {
            this.m_logger = logger;
        }

        public int Max { get; }

        protected override void OnConnecting(ITcpClientBase client, OperationEventArgs e)
        {
            Count count = m_ipToCount.GetOrAdd(client.IP, (s) => { return new Count(); });

            if (count.Increment() > this.Max)
            {
                count.Decrement();
                e.IsPermitOperation = false;//表示不许连接
                e.Handled = true;//并且已经处理该消息。
                this.m_logger.Warning($"IP={client.IP}的客户端，连接数达到设置阈值。已拒绝连接。");
                return;
            }
            base.OnConnecting(client, e);
        }

        protected override void OnDisconnected(ITcpClientBase client, DisconnectEventArgs e)
        {
            if (m_ipToCount.TryGetValue(client.IP, out Count count))
            {
                if (count.Decrement() == 0)
                {
                    m_ipToCount.TryRemove(client.IP, out _);
                }
            }
            base.OnDisconnected(client, e);
        }
    }
}