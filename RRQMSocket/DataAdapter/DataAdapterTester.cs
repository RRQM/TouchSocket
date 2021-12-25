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
        private readonly ConcurrentQueue<TransferByte> asyncBytes;

        private readonly Thread sendThread;

        private readonly EventWaitHandle waitHandle;

        private DataHandlingAdapter adapter;

        private bool sending;

        private bool dispose;

        private int bufferLength;

        private DataAdapterTester()
        {
            asyncBytes = new ConcurrentQueue<TransferByte>();
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
        /// <param name="bufferLength"></param>
        /// <returns></returns>
        public static DataAdapterTester CreateTester(DataHandlingAdapter adapter, Action<ByteBlock, object> receivedCallBack, int bufferLength = 1024)
        {
            DataAdapterTester tester = new DataAdapterTester();
            tester.adapter = adapter;
            tester.bufferLength = bufferLength;
            adapter.SendCallBack = tester.SendCallback;
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
                if (this.asyncBytes.TryDequeue(out TransferByte asyncByte))
                {
                    if (block == null)
                    {
                        block = BytePool.GetByteBlock(bufferLength);
                        byteBlocks.Add(block);
                    }
                    int surLen = bufferLength - block.Pos;
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

        private ByteBlock Write(TransferByte transferByte, ref int offset)
        {
            ByteBlock block = BytePool.GetByteBlock(bufferLength, true);
            int len = Math.Min(transferByte.Length - offset, bufferLength);
            block.Write(transferByte.Buffer, offset, len);
            offset += len;

            return block;
        }

        private void SendCallback(byte[] buffer, int offset, int length, bool isAsync)
        {
            TransferByte asyncByte = new TransferByte(new byte[length], 0, length);
            Array.Copy(buffer, offset, asyncByte.Buffer, 0, length);
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