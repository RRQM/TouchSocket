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
using System.Net;
using System.Text;
using System.Threading;
using TouchSocket.Core.Config;
using TouchSocket.Sockets;

namespace RRQMClient.UDP
{
    public static class UDPDemo
    {
        public static void TestUdpPackage()
        {
            UdpSession udpSession = new UdpSession();
            udpSession.SetDataHandlingAdapter(new UdpPackageAdapter() { MTU = 1472 });
            //udpSession.SetDataHandlingAdapter(new NormalUdpDataHandlingAdapter() {});
            //int receivedCount = 0;
            //udpSession.Received += (remote, byteBlock, requestInfo) =>
            //{
            //    receivedCount++;
            //};

            //udpSession.Setup(new TouchSocketConfig()
            //    .SetBindIPHost(new IPHost(7790))
            //    .SetBufferLength(1024 * 64)
            //    ).Start();

            //EndPoint endPoint = new IPHost("127.0.0.1:7789").EndPoint;
            EndPoint endPoint = new IPHost("127.0.0.1:7789").EndPoint;

            byte[] data = new byte[1024 * 100];
            for (int i = 0; i < 255; i++)
            {
                data[i] = (byte)i;
            }

            while (true)
            {
                Console.WriteLine("按任意键开始测试");
                Console.ReadKey();
                for (int i = 0; i < 100; i++)
                {
                    udpSession.Send(endPoint, data);
                }
            }
        }

        public static void TestUdpPerformance()
        {
            UdpSession udpSession = new UdpSession();

            int receivedCount = 0;
            udpSession.Received += (remote, byteBlock, requestInfo) =>
            {
                receivedCount++;
            };

            udpSession.Setup(new TouchSocketConfig()
                .SetBindIPHost(new IPHost(7790))
                ).Start();

            //EndPoint endPoint = new IPHost("127.0.0.1:7789").EndPoint;
            EndPoint endPoint = new IPHost("127.0.0.1:7789").EndPoint;

            int testCount = 10000;
            while (true)
            {
                Console.WriteLine("按任意键开始测试");
                Console.ReadKey();
                for (int i = 0; i < testCount; i++)
                {
                    udpSession.Send(endPoint, BitConverter.GetBytes(i));
                }

                Thread.Sleep(1000);

                Console.WriteLine($"已发送{testCount}条记录，收到：{receivedCount}条");
                receivedCount = 0;
            }
        }

        public static void TestUdpSession()
        {
            UdpSession udpSession = new UdpSession();
            udpSession.Received += (remote, byteBlock, requestInfo) =>
            {
                Console.WriteLine($"收到：{Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len)}");
            };

            udpSession
                .Setup(new TouchSocketConfig())
                .Start();

            while (true)
            {
                udpSession.Send(new IPHost("127.0.0.1:7789").EndPoint, Encoding.UTF8.GetBytes(Console.ReadLine()));
            }
        }
    }
}