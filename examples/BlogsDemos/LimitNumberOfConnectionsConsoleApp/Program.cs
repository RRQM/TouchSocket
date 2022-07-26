﻿using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Core.Plugins;
using TouchSocket.Sockets;
using TouchSocket.Sockets.Plugins;

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
                .SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
                .SetMaxCount(10000)
                .SetThreadCount(100)
                .UsePlugin())
                .Start();//启动

            service.AddPlugin<LimitNumberOfConnectionsPlugin>();

            Console.ReadKey();
        }
    }

    internal class LimitNumberOfConnectionsPlugin : TcpPluginBase
    {
        [DependencyInject(2)]
        public LimitNumberOfConnectionsPlugin(int max, ILog logger)
        {
            this.Max = max;
            this.Logger = logger;

            logger.Message($"限制连接插件生效，同一IP限制{max}个连接");
        }

        private readonly ConcurrentDictionary<string, Count> m_ipToCount = new ConcurrentDictionary<string, Count>();

        public int Max { get; }

        protected override void OnConnecting(ITcpClientBase client, ClientOperationEventArgs e)
        {
            Count count = m_ipToCount.GetOrAdd(client.IP, (s) => { return new Count(); });

            if (count.Increment() > this.Max)
            {
                count.Decrement();
                e.IsPermitOperation = false;//表示不许连接
                e.Handled = true;//并且已经处理该消息。
                this.Logger.Warning($"IP={client.IP}的客户端，连接数达到设置阈值。已拒绝连接。");
                return;
            }
            base.OnConnecting(client, e);
        }

        protected override void OnDisconnected(ITcpClientBase client, ClientDisconnectedEventArgs e)
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

    internal class Count
    {
        private int num;

        public int Num
        {
            get { return num; }
        }

        public int Increment()
        {
            return Interlocked.Increment(ref num);
        }

        public int Decrement()
        {
            return Interlocked.Decrement(ref num);
        }
    }
}