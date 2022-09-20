//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Diagnostics;
using TouchSocket.Core.Plugins;
using TouchSocket.Sockets;
using TouchSocket.Sockets.Plugins;

namespace RRQMClient.TCP
{
    public static class TCPDemo
    {
        public static void Start()
        {
            Console.WriteLine("1.IOCP简单TCP客户端");
            Console.WriteLine("2.测试TCP客户端连接性能");
            Console.WriteLine("3.测试TCP客户端发送流量性能");
            Console.WriteLine("4.测试同步发送并等待返回TCP客户端");
            Console.WriteLine("5.测试断线重连TCP客户端");
            switch (Console.ReadLine())
            {
                case "1":
                    {
                        TCPDemo.StartSimpleTcpClient();
                        break;
                    }
                case "2":
                    {
                        TCPDemo.StartConnectPerformanceTcpClient();
                        break;
                    }
                case "3":
                    {
                        TCPDemo.StartFlowPerformanceTcpClient();
                        break;
                    }
                case "4":
                    {
                        TCPDemo.CreateSendThenReturnTcpClient();
                        break;
                    }
                case "5":
                    {
                        TCPDemo.CreateBreakResumeTcpClient();
                        break;
                    }
                default:
                    break;
            }
        }

        public static void CreateBreakResumeTcpClient()
        {
            TcpClient tcpClient = new TcpClient();

            tcpClient.Connected += (client, e) =>
            {
                Console.WriteLine("成功连接");
            };
            tcpClient.Disconnected += (client, e) =>
            {
                Console.WriteLine("断开连接");
                //client.Connect();
            };
            tcpClient.Received += (client, byteBlock, requestInfo) =>
            {
                Console.WriteLine($"收到信息：{Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len)}");
            };

            tcpClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .UsePlugin()
                .ConfigurePlugins(a =>
                {
                    a.UseReconnection(-1,true,1000);
                }));

            tcpClient.Connect();

            Console.ReadKey();
        }

        public static void CreatePollBreakResumeTcpClient()
        {
            TcpClient tcpClient = new TcpClient();

            tcpClient.Connected += (client, e) =>
            {
                Console.WriteLine("成功连接");
            };
            tcpClient.Disconnected += (client, e) =>
            {
                Console.WriteLine("断开连接");
            };
            tcpClient.Received += (client, byteBlock, requestInfo) =>
            {
                Console.WriteLine($"收到信息：{Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len)}");
            };

            tcpClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .UsePlugin()
                .ConfigurePlugins(a =>
                {
                    a.Add<PollingKeepAlivePlugin<TcpClient>>();
                })) ;

            tcpClient.Connect();

            Console.ReadKey();
        }

        public static void CreateSendThenReturnTcpClient()
        {
            TcpClient tcpClient = new TcpClient();

            tcpClient.Setup("127.0.0.1:7789");

            tcpClient.Connect();

            Console.WriteLine("输入信息，回车发送");
            while (true)
            {
                byte[] data = tcpClient.GetWaitingClient().SendThenReturn(Encoding.UTF8.GetBytes(Console.ReadLine()));
                Console.WriteLine($"同步收到：{Encoding.UTF8.GetString(data)}");
            }
        }

        public static void StartSimpleTcpClient()
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connecting += (client, e) =>
            {
                //client.SetDataHandlingAdapter(new FixedHeaderPackageAdapter() { FixedHeaderType = FixedHeaderType.Int });
            };

            tcpClient.Connected += (client, e) =>
            {
                Console.WriteLine(e.Message);
                //成功连接到服务器
            };
            tcpClient.Received += (client, byteBlock, obj) =>
            {
                //从服务器收到信息
                string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                Console.WriteLine($"已从服务器接收到信息：{mes}");
            };

            tcpClient.Disconnected += (client, e) =>
            {
                //从服务器断开连接，当连接不成功时不会触发。
                Console.WriteLine(e.Message);
            };

            tcpClient.Setup("tcp://3294b2b361.wicp.vip:28313");

            tcpClient.Connect();

            Console.WriteLine("输入信息，回车发送");
            while (true)
            {
                tcpClient.Send(Encoding.UTF8.GetBytes(Console.ReadLine()));
            }
        }

        public static void StartConnectPerformanceTcpClient()
        {
            Console.WriteLine("按Enter键连接1000个客户端，按其他键，退出测试。");
            List<IClient> clients = new List<IClient>();

            while (true)
            {
                if (Console.ReadKey().Key != ConsoleKey.Enter)
                {
                    break;
                }
                TimeSpan timeSpan = TimeMeasurer.Run(() =>
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        TcpClient tcpClient = new TcpClient();

                        tcpClient.Setup(new TouchSocketConfig()
                           .SetRemoteIPHost(new IPHost("127.0.0.1:7789"))
                           .SetReceiveType(ReceiveType.None));//测试连接性能，启用仅发送功能，此时客户端不投递接收申请。

                        tcpClient.Connect();
                        clients.Add(tcpClient);
                        Console.WriteLine("连接成功");
                    }
                });

                Console.WriteLine($"测试完成，用时:{timeSpan}");
            }
        }

        public static void StartFlowPerformanceTcpClient()
        {
            TcpClient tcpClient = new TcpClient();

            Console.WriteLine("输入iphost");

            tcpClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost(new IPHost("127.0.0.1:7789"))
                .SetReceiveType(ReceiveType.None))
                .Connect();

            Console.WriteLine("连接成功");

            byte[] buffer = new byte[10240];
            new Random().NextBytes(buffer);

            while (true)
            {
                for (int i = 0; i < 1; i++)
                {
                    tcpClient.SendAsync(buffer);
                }
                Console.ReadKey();
            }
        }
    }

    public class MyClient : TcpClient
    {
        protected override void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            //此处处理数据，功能相当于Received事件。
            string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
            Console.WriteLine($"已接收到信息：{mes}");
        }
    }

    public class MyTClient : TcpClient
    {
        protected override void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            byteBlock.SetHolding(true);//异步前锁定
            Task.Run(() =>
            {
                string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                byteBlock.SetHolding(false);//使用完成后取消锁定，且不用再调用Dispose
                Console.WriteLine($"已接收到信息：{mes}");
            });
        }
    }

    public static class DependencyExtensions
    {
        /// <summary>
        /// 依赖项
        /// </summary>
        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.Register("MyProperty", typeof(int), typeof(DependencyExtensions), 10);

        /// <summary>
        /// 设置MyProperty
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TClient SetMyProperty<TClient>(this TClient client, int value) where TClient : IClient
        {
            client.SetValue(MyPropertyProperty, value);
            return client;
        }

        /// <summary>
        /// 获取MyProperty
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        public static int GetMyProperty<TClient>(this TClient client) where TClient : IClient
        {
            return client.GetValue<int>(MyPropertyProperty);
        }
    }
}