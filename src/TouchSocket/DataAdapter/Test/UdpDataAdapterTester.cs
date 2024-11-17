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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// Udp数据处理适配器测试
    /// </summary>
    public class UdpDataAdapterTester : IDisposable
    {
        private readonly IntelligentDataQueue<QueueDataBytes> m_asyncBytes;
        private UdpDataHandlingAdapter m_adapter;
        private int m_count;
        private bool m_dispose;
        private int m_expectedCount;
        private Func<ByteBlock, IRequestInfo, Task> m_receivedCallBack;
        private Stopwatch m_stopwatch;
        private int m_millisecondsTimeout;

        private UdpDataAdapterTester(int multiThread)
        {
            this.m_asyncBytes = new IntelligentDataQueue<QueueDataBytes>(1024 * 1024 * 10);
            for (var i = 0; i < multiThread; i++)
            {
                Task.Run(this.BeginSend);
            }
        }

        /// <summary>
        /// 获取测试器
        /// </summary>
        /// <param name="adapter">待测试适配器</param>
        /// <param name="multiThread">并发多线程数量</param>
        /// <param name="receivedCallBack">收到数据回调</param>
        /// <returns></returns>
        public static UdpDataAdapterTester CreateTester(UdpDataHandlingAdapter adapter, int multiThread, Func<ByteBlock, IRequestInfo, Task> receivedCallBack = default)
        {
            var tester = new UdpDataAdapterTester(multiThread);
            tester.m_adapter = adapter;
            adapter.SendCallBackAsync = tester.SendCallback;
            adapter.ReceivedCallBack = tester.OnReceived;
            tester.m_receivedCallBack = receivedCallBack;
            return tester;
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            this.m_dispose = true;
        }

        /// <summary>
        /// 模拟测试运行发送
        /// </summary>
        /// <param name="memory">待测试的内存块</param>
        /// <param name="testCount">测试次数</param>
        /// <param name="expectedCount">期待测试次数</param>
        /// <param name="millisecondsTimeout">超时时间（毫秒）</param>
        /// <returns>测试运行的时间差</returns>
        public TimeSpan Run(ReadOnlyMemory<byte> memory, int testCount, int expectedCount, int millisecondsTimeout)
        {
            this.m_count = 0;
            this.m_expectedCount = expectedCount;
            this.m_millisecondsTimeout = millisecondsTimeout;
            this.m_stopwatch = new Stopwatch();
            this.m_stopwatch.Start();
            Task.Run(async () =>
            {
                for (var i = 0; i < testCount; i++)
                {
                    await this.m_adapter.SendInputAsync(null, memory).ConfigureAwait(false);
                }
            });
            if (SpinWait.SpinUntil(() => this.m_count == this.m_expectedCount, this.m_millisecondsTimeout))
            {
                this.m_stopwatch.Stop();
                return this.m_stopwatch.Elapsed;
            }

            throw new TimeoutException();
        }


        private async Task BeginSend()
        {
            while (!this.m_dispose)
            {
                if (this.TryGet(out var byteBlocks))
                {
                    foreach (var block in byteBlocks)
                    {
                        try
                        {
                            await this.m_adapter.ReceivedInput(null, block).ConfigureAwait(false);
                        }
                        finally
                        {
                            block.Dispose();
                        }
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        private async Task OnReceived(EndPoint endPoint, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.m_receivedCallBack != null)
            {
                await this.m_receivedCallBack(byteBlock, requestInfo).ConfigureAwait(false);
            }
            Interlocked.Increment(ref this.m_count);
        }

        private Task SendCallback(EndPoint endPoint, ReadOnlyMemory<byte> memory)
        {
            var array = memory.ToArray();
            var asyncByte = new QueueDataBytes(array, 0, array.Length);
            //Array.Copy(buffer, offset, asyncByte.Buffer, 0, length);
            this.m_asyncBytes.Enqueue(asyncByte);
            return EasyTask.CompletedTask;
        }

        private bool TryGet(out List<ByteBlock> byteBlocks)
        {
            byteBlocks = new List<ByteBlock>();

            while (this.m_asyncBytes.TryDequeue(out var asyncByte))
            {
                var block = new ByteBlock(asyncByte.Length);
                block.Write(new ReadOnlySpan<byte>(asyncByte.Buffer, asyncByte.Offset, asyncByte.Length));
                byteBlocks.Add(block);
            }
            return byteBlocks.Count > 0;
        }
    }
}