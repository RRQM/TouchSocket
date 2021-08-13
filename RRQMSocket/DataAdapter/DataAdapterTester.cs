//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMCore.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace RRQMSocket
{
    /// <summary>
    /// 数据处理适配器测试
    /// </summary>
    public class DataAdapterTester : IDisposable
    {
        private readonly ConcurrentQueue<AsyncByte> asyncBytes;

        private readonly Thread sendThread;

        private readonly EventWaitHandle waitHandle;

        private DataHandlingAdapter adapter;

        private bool sending;

        private bool dispose;

        private int bufferLength;

        private DataAdapterTester()
        {
            asyncBytes = new ConcurrentQueue<AsyncByte>();
            waitHandle = new AutoResetEvent(false);
            this.sendThread = new Thread(this.BeginSend);
            this.sendThread.IsBackground = true;
            this.sendThread.Name = "DataAdapterTesterThread";
            this.sendThread.Start();
        }

        /// <summary>
        /// 获取测试器
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="receivedCallBack"></param>
        /// <param name="logger"></param>
        /// <param name="bufferLength"></param>
        /// <returns></returns>
        public static DataAdapterTester CreateTester(DataHandlingAdapter adapter, Action<ByteBlock, object> receivedCallBack, ILog logger, int bufferLength = 1024)
        {
            DataAdapterTester tester = new DataAdapterTester();
            tester.adapter = adapter;
            tester.bufferLength = bufferLength;

            adapter.Logger = logger;
            adapter.SendCallBack = tester.SendCallback;
            adapter.BytePool = new BytePool();
            adapter.ReceivedCallBack = receivedCallBack;

            return tester;
        }

        /// <summary>
        /// 模拟发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void SimSend(byte[] buffer, int offset, int length)
        {
            adapter.Send(buffer, offset, length, false);
        }

        /// <summary>
        /// 模拟发送
        /// </summary>
        /// <param name="buffer"></param>
        public void SimSend(byte[] buffer)
        {
            this.SimSend(buffer, 0, buffer.Length);
        }

        private void BeginSend()
        {
            while (!this.dispose)
            {
                if (this.tryGet(out List<ByteBlock> byteBlocks))
                {
                    this.sending = true;

                    foreach (var block in byteBlocks)
                    {
                        try
                        {
                            this.adapter.Received(block);
                        }
                        finally
                        {
                            block.Dispose();
                        }
                    }
                }
                else
                {
                    this.sending = false;
                    this.waitHandle.WaitOne();
                }
            }
        }

        private bool tryGet(out List<ByteBlock> byteBlocks)
        {
            byteBlocks = new List<ByteBlock>();

            ByteBlock block = null;

            while (true)
            {
                if (this.asyncBytes.TryDequeue(out AsyncByte asyncByte))
                {
                    if (block == null)
                    {
                        block = this.adapter.BytePool.GetByteBlock(bufferLength);
                        byteBlocks.Add(block);
                    }

                    int surLen = block.Capacity - (int)block.Position;
                    if (surLen >= asyncByte.length)
                    {
                        block.Write(asyncByte.buffer, asyncByte.offset, asyncByte.length);
                    }
                    else
                    {
                        block.Write(asyncByte.buffer, asyncByte.offset, surLen);

                        int surDataLen = asyncByte.length - surLen;
                        int offset = asyncByte.offset + surLen;

                        while (surDataLen > 0)
                        {
                            block = this.adapter.BytePool.GetByteBlock(bufferLength);
                            byteBlocks.Add(block);
                            int len = Math.Min(surDataLen, bufferLength);
                            block.Write(asyncByte.buffer, offset, len);
                            surDataLen -= len;
                            offset += len;
                        }

                        break;
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

        private void SendCallback(byte[] buffer, int offset, int length, bool isAsync)
        {
            AsyncByte asyncByte = new AsyncByte();
            asyncByte.buffer = new byte[length];

            Array.Copy(buffer, offset, asyncByte.buffer, 0, length);
            asyncByte.offset = 0;
            asyncByte.length = length;
            this.asyncBytes.Enqueue(asyncByte);
            if (!this.sending)
            {
                this.waitHandle.Set();
            }
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            this.dispose = true;
            this.waitHandle.Set();
            this.waitHandle.Dispose();
        }
    }
}