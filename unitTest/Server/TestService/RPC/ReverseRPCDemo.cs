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
using System.Threading.Tasks;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Diagnostics;
using TouchSocket.Rpc;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace RRQMService.RPC
{
    public static class ReverseRpcDemo
    {
        public static void TestSleepPerformance()
        {
            RpcStore service = new RpcStore(new Container());

            TcpTouchRpcService tcpRPCParser = new TcpTouchRpcService();
            service.AddRpcParser("tcpRPCParser", tcpRPCParser);
            service.RegisterServer<PerformanceServer>();

            //tcpRPCParser.Handshaked += (client, e) =>//在客户端握手完成连接时，就反向调用，同时正向RPC由客户端完成。
            //{
            //    Task.Run(() =>
            //    {
            //        int i = 0;
            //        while (true)
            //        {
            //            if (i % 100 == 0)
            //            {
            //                Console.WriteLine(i);
            //            }
            //            int value = client.Invoke<int>("ConPerformance", InvokeOption.WaitInvoke, i++);
            //            if (value != i)
            //            {
            //                Console.WriteLine("调用结果不一致");
            //            }
            //            //await Task.Delay(10);
            //        }
            //    });
            //};

            tcpRPCParser.Setup(new TouchSocketConfig()
                .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                .SetVerifyToken("123RPC"))
                .Start();

            Console.WriteLine("服务已启动");
            Console.ReadKey();
        }

        public static void TestPerformance()
        {
            TcpTouchRpcService tcpRpcParser = new TcpTouchRpcService();
            //tcpRpcParser.Handshaked += (client, e) =>
            //{
            //    Task.Run(() =>
            //    {
            //        TimeSpan timeSpan = TimeMeasurer.Run(() =>
            //        {
            //            for (int i = 0; i < 100000; i++)
            //            {
            //                if (i % 1000 == 0)
            //                {
            //                    Console.WriteLine(i);
            //                }
            //                int value = client.Invoke<int>("ConPerformance", InvokeOption.WaitInvoke, i);
            //                if (value != i + 1)
            //                {
            //                    Console.WriteLine("调用结果不一致");
            //                }
            //            }
            //        });
            //        Console.WriteLine($"测试完成，用时{timeSpan}");
            //    });
            //};

            tcpRpcParser.Setup(new TouchSocketConfig()
                .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                .SetVerifyToken("123RPC"))
                .Start();

            Console.WriteLine("服务已启动");
            Console.ReadKey();
        }
    }

    public class PerformanceServer : RpcServer
    {
        [TouchRpc]
        public int ConPerformance(int age)
        {
            return ++age;
        }
    }
}