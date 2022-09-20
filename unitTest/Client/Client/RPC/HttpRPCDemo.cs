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
using System.Threading;
using TouchSocket.Core.Config;
using TouchSocket.Core.Diagnostics;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace RRQMClient.RPC
{
    public class HttpRPCDemo
    {
        public static void StartConnectPerformanceTokenClient()
        {
            Console.WriteLine("按Enter键连接1000个客户端，按其他键，退出测试。");
            List<IClient> clients = new List<IClient>();
            ThreadPool.SetMinThreads(50, 50);
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
                        HttpTouchRpcClient client = new HttpTouchRpcClient();

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
    }
}