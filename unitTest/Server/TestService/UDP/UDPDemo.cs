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
using System.Net;
using System.Text;
using System.Threading;
using TouchSocket.Core.Config;
using TouchSocket.Core.Run;
using TouchSocket.Sockets;

namespace RRQMService.UDP
{
    public static class UDPDemo
    {
        public static void TestUdpMulticastGroup()
        {
            IPEndPoint multicast = new IPEndPoint(IPAddress.Parse("224.5.6.7"), 7788);

            UdpSession mainUdpSession = new UdpSession();
            mainUdpSession.SetDataHandlingAdapter(new UdpPackageAdapter());
            //udpSession.SetDataHandlingAdapter(new NormalUdpDataHandlingAdapter());

            mainUdpSession.Setup(new TouchSocketConfig()
                .SetBindIPHost(new IPHost(7789))
                .SetThreadCount(1)
                .SetBufferLength(1024 * 64))
                .Start();

            mainUdpSession.JoinMulticastGroup(IPAddress.Parse("224.5.6.7"));

            UdpSession multicastUdpSession = new UdpSession();
            multicastUdpSession.SetDataHandlingAdapter(new UdpPackageAdapter());
            //udpSession.SetDataHandlingAdapter(new NormalUdpDataHandlingAdapter());

            int count = 0;
            multicastUdpSession.Received += (remote, byteBlock, requestInfo) =>
            {
                Console.WriteLine(byteBlock.ToString());
                count++;
                if (count == 5)
                {
                    multicastUdpSession.DropMulticastGroup(IPAddress.Parse("224.5.6.7"));
                }
            };

            multicastUdpSession.Setup(new TouchSocketConfig()
                .SetBindIPHost(new IPHost(7788))
                .SetThreadCount(1)
                .SetBufferLength(1024 * 64))
                .Start();

            multicastUdpSession.JoinMulticastGroup(IPAddress.Parse("224.5.6.7"));

            while (true)
            {
                mainUdpSession.Send(multicast, Encoding.UTF8.GetBytes("RRQM"));
                Thread.Sleep(1000);
            }
        }

        public static void TestUdpPackage()
        {
            UdpSession udpSession = new UdpSession();
            udpSession.SetDataHandlingAdapter(new UdpPackageAdapter() { Timeout = 60 * 1000 });
            //udpSession.SetDataHandlingAdapter(new NormalUdpDataHandlingAdapter());

            List<byte[]> bytes = new List<byte[]>();
            int count = 0;
            Dictionary<EndPoint, int> pairs = new Dictionary<EndPoint, int>();

            udpSession.Received += (remote, byteBlock, requestInfo) =>
            {
                // Console.WriteLine($"收到：{byteBlock.Len}");
                //endPoint = remote;
                Interlocked.Increment(ref count);
                //bytes.Add(byteBlock.ToArray());
                //ushort sn = RRQMBitConverter.Default.ToUInt16(byteBlock.Buffer, 1);
                //Console.WriteLine($"ID={byteBlock.Buffer[0]},SN={sn}");
                //udpSession.Send(remote, byteBlock);
            };

            udpSession.Setup(new TouchSocketConfig()
                .SetBindIPHost(new IPHost(7789))
                .SetThreadCount(10)
                .SetBufferLength(1024 * 64))
                .Start();

            ThreadPool.SetMinThreads(20, 20);
            LoopAction loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
              {
                  Console.WriteLine(count);
                  count = 0;
              });
            loopAction.RunAsync();
            Console.WriteLine("等待接收");
        }

        public static void TestUdpPerformance()
        {
            UdpSession udpSession = new UdpSession();

            udpSession.Received += (remote, byteBlock, requestInfo) =>
            {
                udpSession.Send(remote, byteBlock);
            };

            udpSession.Setup(new TouchSocketConfig()
                .SetBindIPHost(new IPHost(7789)))
                .Start();
            Console.WriteLine("等待接收");
        }

        public static void TestUdpSession()
        {
            UdpSession udpSession = new UdpSession();
            udpSession.Received += (remote, byteBlock, requestInfo) =>
            {
                udpSession.Send(remote, byteBlock);
                Console.WriteLine($"从{remote}收到：{Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len)}");
            };
            udpSession.Setup(new TouchSocketConfig()
                 .SetBindIPHost(new IPHost(7789)))
                 .Start();
            Console.WriteLine("等待接收");
        }
    }
}