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
using RRQMProxy;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Diagnostics;
using TouchSocket.Core.Run;
using TouchSocket.Core.Serialization;
using TouchSocket.Rpc;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace RRQMClient.RPC
{
    public static class RPCDemo
    {
        private static TcpTouchRpcClient GetTcpRpcClient()
        {
            TcpTouchRpcClient client = new TcpTouchRpcClient();

            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost(new IPHost("127.0.0.1:7789"))
                .SetVerifyToken("123RPC")
                //.UseDelaySender()
                .SetMaxCount(50)
                );

            try
            {
                client.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return client;
        }

        public static void Test_IDInvoke()
        {
            TcpTouchRpcClient client1 = GetTcpRpcClient();
            TcpTouchRpcClient client2 = GetTcpRpcClient();

            RpcStore service = new RpcStore(new Container());
            service.AddRpcParser("client1", client1);
            service.RegisterServer<CallbackServer>();

            TimeSpan timeSpan = TimeMeasurer.Run(() =>
            {
                for (int j = 0; j < 100000; j++)
                {
                    int result = client2.Invoke<int>(client1.ID, "Performance", InvokeOption.WaitInvoke, j);
                    if (result != j + 1)
                    {
                        Console.WriteLine("调用结果不一致。");
                    }
                    if (j % 1000 == 0)
                    {
                        Console.WriteLine($"已调用{j}次");
                    }
                }
            });

            Console.WriteLine($"测试结束。用时：{timeSpan}");
            Console.ReadKey();
        }

        public static void Test_ConPerformance()
        {
            TcpTouchRpcClient tcpRpcClient = GetTcpRpcClient();

            PerformanceRpcServer rpcServer = new PerformanceRpcServer(tcpRpcClient);
            Console.WriteLine("请输入待测试并发数量");
            int clientCount = int.Parse(Console.ReadLine());
            /*
             并发性能测试内容为，同时多个异步调用同一个方法，
             然后检测其返回值。
             */
            for (int i = 0; i < clientCount; i++)
            {
                Task.Run(() =>
                {
                    TimeSpan timeSpan = TimeMeasurer.Run(() =>
                    {
                        for (int j = 0; j < 100000; j++)
                        {
                            try
                            {
                                int result = tcpRpcClient.Invoke<int>("ConPerformance", InvokeOption.WaitInvoke, j);
                                if (result != j + 1)
                                {
                                    Console.WriteLine("调用结果不一致");
                                }
                                if (j % 1000 == 0)
                                {
                                    Console.WriteLine($"已调用{j}次");
                                }
                            }
                            catch (Exception ex)
                            {

                               
                            }
                           
                        }
                    });

                    Console.WriteLine($"测试结束。用时：{timeSpan}");
                });
            }

            Console.ReadKey();
        }

        //public static void Test_Ping()
        //{
        //    TcpTouchRpcClient tcpRpcClient = GetTcpRpcClient();

        //    for (int i = 0; i < clientCount; i++)
        //    {
        //        Task.Run(() =>
        //        {
        //            TimeSpan timeSpan = RRQMCore.Diagnostics.TimeMeasurer.Run(() =>
        //            {
        //                for (int j = 0; j < 100000; j++)
        //                {
        //                    int result = tcpRpcClient.Invoke<int>("ConPerformance", InvokeOption.WaitInvoke, j);
        //                    if (result != j + 1)
        //                    {
        //                        Console.WriteLine("调用结果不一致");
        //                    }
        //                    if (j % 1000 == 0)
        //                    {
        //                        Console.WriteLine($"已调用{j}次");
        //                    }
        //                }
        //            });

        //            Console.WriteLine($"测试结束。用时：{timeSpan}");
        //        });
        //    }

        //    Console.ReadKey();
        //}

        public static void Test_ElapsedTimeRpcServer()
        {
            TcpTouchRpcClient tcpRpcClient = GetTcpRpcClient();

            ElapsedTimeRpcServer rpcServer = new ElapsedTimeRpcServer(tcpRpcClient);

            int tick = 100;

            Console.WriteLine($"已调用{nameof(rpcServer.DelayInvoke)},延迟{tick * 100}毫秒后完成");
            Console.WriteLine("按任意键取消任务。");

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            Task.Run(() =>
            {
                InvokeOption invokeOption = new InvokeOption();
                invokeOption.Timeout = 1000 * 60;
                invokeOption.FeedbackType = FeedbackType.WaitInvoke;
                invokeOption.SerializationType = SerializationType.FastBinary;

                invokeOption.Token = tokenSource.Token;

                //实际上当为false时，并不是返回的值，而是default值
                bool status = rpcServer.DelayInvoke(tick, invokeOption);
                Console.WriteLine(status);
            });

            Console.ReadKey();
            tokenSource.Cancel();
        }

        public static void StartConnectPerformanceTokenClient()
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
                        TcpTouchRpcClient client = new TcpTouchRpcClient();

                        client.Setup(new TouchSocketConfig()
                            .SetRemoteIPHost("127.0.0.1:7789")
                            .SetVerifyToken("123RPC"));

                        client.Connect();
                        clients.Add(client);
                        Console.WriteLine("连接成功");
                    }
                });

                Console.WriteLine($"测试完成，用时:{timeSpan}");
            }
        }

        public static void Test_PerformanceRpcServer()
        {
            int count = 0;

            Console.WriteLine("请输入待测试客户端数量");
            int clientCount = int.Parse(Console.ReadLine());
            ThreadPool.SetMinThreads(clientCount + 1, clientCount + 1);

            bool end = false;
            for (int i = 0; i < clientCount; i++)
            {
                TcpTouchRpcClient client = GetTcpRpcClient();
                PerformanceRpcServer rpcServer = new PerformanceRpcServer(client);

                Task.Run(() =>
                {
                    while (!end)
                    {
                        rpcServer.Performance(InvokeOption.WaitInvoke);
                        count++;
                    }
                });
            }

            LoopAction loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
            {
                Console.WriteLine($"调用{count}次");
                count = 0;
            });
            loopAction.RunAsync();

            while (true)
            {
                Console.ReadKey();
                end = true;
                GC.Collect();
                BytePool.Clear();
                Console.WriteLine("已清空内存池");
            }
        }
    }

    public class CallbackServer : TouchSocket.Rpc.RpcServer
    {
        [TouchRpc]
        public int Performance(int i)
        {
            return ++i;
        }
    }
}