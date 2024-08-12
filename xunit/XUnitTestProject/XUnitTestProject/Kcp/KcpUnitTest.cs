//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using TouchSocket.Core;
//using TouchSocket.Kcp;
//using Xunit;

//namespace XUnitTest.Kcp
//{
//    public class KcpUnitTest
//    {
//        [Theory]
//        [InlineData(1)]
//        [InlineData(10)]
//        [InlineData(100)]
//        [InlineData(1000)]
//        [InlineData(10000)]
//        [InlineData(100000)]
//        public void KcpCoreShouldBeOk(int testCount)
//        {
//            KcpCore kcpServer = new KcpCore()
//            {
//                Conv = 1,
//                Interval = 1,
//                Rcv_wnd = 10000,
//                Snd_wnd = 10000
//            };

//            KcpCore kcpClient = new KcpCore()
//            {
//                Conv = 1,
//                Interval = 1,
//                Rcv_wnd = 10000,
//                Snd_wnd = 10000
//            };

//            byte[] randomBytes = new byte[1024];
//            new Random().NextBytes(randomBytes);
//            int index = 0;
//            kcpServer.Output = (buffer, len) =>
//            {
//                if (Interlocked.Increment(ref index)>=1024)
//                {
//                    index = 0;
//                }
//                if (randomBytes[index]%10==0)
//                {
//                    return;//丢包
//                }
//                kcpClient.Input(buffer, 0, len);
//            };

//            kcpClient.Output = (buffer, len) =>
//            {
//                if (Interlocked.Increment(ref index) >= 1024)
//                {
//                    index = 0;
//                }
//                if (randomBytes[index] % 10 == 0)
//                {
//                    return;//丢包
//                }
//                kcpServer.Input(buffer, 0, len);
//            };

//            byte[] data = new byte[1024*5];

//            for (int i = 0; i < testCount; i++)
//            {
//                kcpClient.Send(data, 0, data.Length);
//            }

//            byte[] buffer = new byte[1024*1024];

//            int count = 0;
//            int success = 0;
//            int overCount = Math.Max(testCount * 5, 1000);
//            while (true)
//            {
//                kcpServer.Update(DateTime.Now.ConvertTime());
//                kcpClient.Update(DateTime.Now.ConvertTime());
//                int r = kcpServer.Receive(buffer, buffer.Length);
//                if (++count > overCount)
//                {
//                    break;
//                }
//                if (r < 0)
//                {
//                    Thread.Sleep(10);
//                    continue;
//                }

//                Assert.True(r == data.Length);

//                for (int j = 0; j < data.Length; j++)
//                {
//                    Assert.Equal(data[j], buffer[j]);
//                }

//                if (++success == testCount)
//                {
//                    break;
//                }
//            }

//            Assert.True(count < overCount);
//        }
//    }

//    class MyKcpClass
//    {
//    }
//}