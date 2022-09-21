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
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Collections.Concurrent;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 数据处理适配器测试
    /// </summary>
    public class DataAdapterTester : IDisposable
    {
        private readonly IntelligentDataQueue<QueueDataBytes> asyncBytes;
        private readonly Thread sendThread;
        private DataHandlingAdapter adapter;
        private int bufferLength;
        private int count;
        private bool dispose;
        private int expectedCount;
        private Action<ByteBlock, IRequestInfo> receivedCallBack;
        private Stopwatch stopwatch;
        private int timeout;

        private DataAdapterTester()
        {
            this.asyncBytes = new IntelligentDataQueue<QueueDataBytes>(1024 * 1024 * 10);
            this.sendThread = new Thread(this.BeginSend);
            this.sendThread.IsBackground = true;
            this.sendThread.Name = "DataAdapterTesterThread";
            this.sendThread.Start();
        }

        /// <summary>
        /// 获取测试器
        /// </summary>
        /// <param name="adapter">待测试适配器</param>
        /// <param name="receivedCallBack">收到数据回调</param>
        /// <param name="bufferLength">缓存数据长度</param>
        /// <returns></returns>
        public static DataAdapterTester CreateTester(DataHandlingAdapter adapter, int bufferLength = 1024, Action<ByteBlock, IRequestInfo> receivedCallBack = default)
        {
            DataAdapterTester tester = new DataAdapterTester();
            tester.adapter = adapter;
            tester.bufferLength = bufferLength;
            adapter.SendCallBack = tester.SendCallback;
            adapter.ReceivedCallBack = tester.OnReceived;
            tester.receivedCallBack = receivedCallBack;
            return tester;
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            this.dispose = true;
        }

        /// <summary>
        /// 模拟测试运行发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="testCount">测试次数</param>
        /// <param name="expectedCount">期待测试次数</param>
        /// <param name="timeout">超时</param>
        /// <returns></returns>
        public TimeSpan Run(byte[] buffer, int offset, int length, int testCount, int expectedCount, int timeout)
        {
            this.count = 0;
            this.expectedCount = expectedCount;
            this.timeout = timeout;
            this.stopwatch = new Stopwatch();
            this.stopwatch.Start();
            Task.Run(() =>
            {
                for (int i = 0; i < testCount; i++)
                {
                    this.adapter.SendInput(buffer, offset, length, false);
                }
            });

            if (SpinWait.SpinUntil(() => this.count == this.expectedCount, this.timeout))
            {
                this.stopwatch.Stop();
                return this.stopwatch.Elapsed;
            }
            throw new TimeoutException();
        }

        /// <summary>
        /// 模拟发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="testCount">测试次数</param>
        /// <param name="expectedCount">期待测试次数</param>
        /// <param name="timeout">超时</param>
        public TimeSpan Run(byte[] buffer, int testCount, int expectedCount, int timeout)
        {
            return this.Run(buffer, 0, buffer.Length, testCount, expectedCount, timeout);
        }

        private void BeginSend()
        {
            while (!this.dispose)
            {
                if (this.tryGet(out List<ByteBlock> byteBlocks))
                {
                    foreach (var block in byteBlocks)
                    {
                        try
                        {
                            this.adapter.ReceivedInput(block);
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

        private void OnReceived(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.count++;
            this.receivedCallBack?.Invoke(byteBlock, requestInfo);
        }

        private void SendCallback(byte[] buffer, int offset, int length, bool isAsync)
        {
            QueueDataBytes asyncByte = new QueueDataBytes(new byte[length], 0, length);
            Array.Copy(buffer, offset, asyncByte.Buffer, 0, length);
            this.asyncBytes.Enqueue(asyncByte);
        }

        private bool tryGet(out List<ByteBlock> byteBlocks)
        {
            byteBlocks = new List<ByteBlock>();
            ByteBlock block = null;
            while (true)
            {
                if (this.asyncBytes.TryDequeue(out QueueDataBytes asyncByte))
                {
                    if (block == null)
                    {
                        block = BytePool.GetByteBlock(this.bufferLength);
                        byteBlocks.Add(block);
                    }
                    int surLen = this.bufferLength - block.Pos;
                    if (surLen < asyncByte.Length)//不能完成写入
                    {
                        block.Write(asyncByte.Buffer, asyncByte.Offset, surLen);
                        int offset = surLen;
                        while (offset < asyncByte.Length)
                        {
                            block = this.Write(asyncByte, ref offset);
                            byteBlocks.Add(block);
                        }

                        if (byteBlocks.Count > 10)
                        {
                            break;
                        }
                    }
                    else//本次能完成写入
                    {
                        block.Write(asyncByte.Buffer, asyncByte.Offset, asyncByte.Length);
                        if (byteBlocks.Count > 10)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    if (byteBlocks.Count > 0)
                    {
                        break;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private ByteBlock Write(QueueDataBytes transferByte, ref int offset)
        {
            ByteBlock block = BytePool.GetByteBlock(this.bufferLength, true);
            int len = Math.Min(transferByte.Length - offset, this.bufferLength);
            block.Write(transferByte.Buffer, offset, len);
            offset += len;

            return block;
        }
    }

    /// <summary>
    /// Udp数据处理适配器测试
    /// </summary>
    public class UdpDataAdapterTester : IDisposable
    {
        private readonly IntelligentDataQueue<QueueDataBytes> asyncBytes;
        private UdpDataHandlingAdapter adapter;
        private int count;
        private bool dispose;
        private int expectedCount;
        private Action<ByteBlock, IRequestInfo> receivedCallBack;
        private Stopwatch stopwatch;
        private int timeout;

        private UdpDataAdapterTester(int multiThread)
        {
            this.asyncBytes = new IntelligentDataQueue<QueueDataBytes>(1024 * 1024 * 10);
            for (int i = 0; i < multiThread; i++)
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
        public static UdpDataAdapterTester CreateTester(UdpDataHandlingAdapter adapter, int multiThread, Action<ByteBlock, IRequestInfo> receivedCallBack = default)
        {
            UdpDataAdapterTester tester = new UdpDataAdapterTester(multiThread);
            tester.adapter = adapter;
            adapter.SendCallBack = tester.SendCallback;
            adapter.ReceivedCallBack = tester.OnReceived;
            tester.receivedCallBack = receivedCallBack;
            return tester;
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            this.dispose = true;
        }

        /// <summary>
        /// 模拟测试运行发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="testCount">测试次数</param>
        /// <param name="expectedCount">期待测试次数</param>
        /// <param name="timeout">超时</param>
        /// <returns></returns>
        public TimeSpan Run(byte[] buffer, int offset, int length, int testCount, int expectedCount, int timeout)
        {
            this.count = 0;
            this.expectedCount = expectedCount;
            this.timeout = timeout;
            this.stopwatch = new Stopwatch();
            this.stopwatch.Start();
            Task.Run(() =>
            {
                for (int i = 0; i < testCount; i++)
                {
                    this.adapter.SendInput(null, buffer, offset, length, false);
                }
            });
            if (SpinWait.SpinUntil(() => this.count == this.expectedCount, this.timeout))
            {
                this.stopwatch.Stop();
                return this.stopwatch.Elapsed;
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// 模拟发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="testCount">测试次数</param>
        /// <param name="expectedCount">期待测试次数</param>
        /// <param name="timeout">超时</param>
        public TimeSpan Run(byte[] buffer, int testCount, int expectedCount, int timeout)
        {
            return this.Run(buffer, 0, buffer.Length, testCount, expectedCount, timeout);
        }

        private void BeginSend()
        {
            while (!this.dispose)
            {
                if (this.tryGet(out List<ByteBlock> byteBlocks))
                {
                    foreach (var block in byteBlocks)
                    {
                        try
                        {
                            this.adapter.ReceivedInput(null, block);
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

        private void OnReceived(EndPoint endPoint, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.receivedCallBack?.Invoke(byteBlock, requestInfo);
            Interlocked.Increment(ref this.count);
        }

        private void SendCallback(EndPoint endPoint, byte[] buffer, int offset, int length, bool isAsync)
        {
            QueueDataBytes asyncByte = new QueueDataBytes(new byte[length], 0, length);
            Array.Copy(buffer, offset, asyncByte.Buffer, 0, length);
            this.asyncBytes.Enqueue(asyncByte);
        }

        private bool tryGet(out List<ByteBlock> byteBlocks)
        {
            byteBlocks = new List<ByteBlock>();

            while (this.asyncBytes.TryDequeue(out QueueDataBytes asyncByte))
            {
                ByteBlock block = new ByteBlock(asyncByte.Length);
                block.Write(asyncByte.Buffer, asyncByte.Offset, asyncByte.Length);
                byteBlocks.Add(block);
            }
            if (byteBlocks.Count > 0)
            {
                return true;
            }
            return false;
        }
    }
}