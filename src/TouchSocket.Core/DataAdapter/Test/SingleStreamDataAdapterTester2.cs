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
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    ///单线程状况的流式数据处理适配器测试
    /// </summary>
    public class SingleStreamDataAdapterTester<TAdapter, TRequest> : DisposableObject where TAdapter : CustomDataHandlingAdapter<TRequest>
        where TRequest : class, IRequestInfo
    {
        private readonly IntelligentDataQueue<QueueDataBytes> m_asyncBytes;
        private readonly TAdapter m_adapter;
        private readonly int m_bufferLength;
        private readonly Action<TRequest> m_receivedCallBack;
        private int m_count;
        private int m_expectedCount;
        private Stopwatch m_stopwatch;
        private int m_timeout;

        /// <summary>
        /// Tcp数据处理适配器测试
        /// </summary>
        public SingleStreamDataAdapterTester(TAdapter adapter, int bufferLength = 1024, Action<TRequest> receivedCallBack = default)
        {
            this.m_adapter = adapter;
            this.m_asyncBytes = new IntelligentDataQueue<QueueDataBytes>(1024 * 1024 * 10);
            this.m_bufferLength = bufferLength;
            this.m_receivedCallBack = receivedCallBack;
            adapter.SendAsyncCallBack = SendCallback;
            Task.Run(this.BeginSend);
        }


        /// <summary>
        /// 模拟测试运行发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="testCount">测试次数</param>
        /// <param name="expectedCount">期待测试次数</param>
        /// <param name="millisecondsTimeout">超时</param>
        /// <returns></returns>
        public TimeSpan Run(byte[] buffer, int offset, int length, int testCount, int expectedCount, int millisecondsTimeout)
        {
            this.m_count = 0;
            this.m_expectedCount = expectedCount;
            this.m_timeout = millisecondsTimeout;
            this.m_stopwatch = new Stopwatch();
            this.m_stopwatch.Start();
            Task.Run(async () =>
            {
                for (var i = 0; i < testCount; i++)
                {
                    await this.m_adapter.SendInputAsync(new Memory<byte>(buffer, offset, length)).ConfigureFalseAwait();
                }
            });

            if (SpinWait.SpinUntil(() => this.m_count == this.m_expectedCount, this.m_timeout))
            {
                this.m_stopwatch.Stop();
                return this.m_stopwatch.Elapsed;
            }
            throw new TimeoutException();
        }

        /// <summary>
        /// 模拟发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="testCount">测试次数</param>
        /// <param name="expectedCount">期待测试次数</param>
        /// <param name="millisecondsTimeout">超时</param>
        public TimeSpan Run(byte[] buffer, int testCount, int expectedCount, int millisecondsTimeout)
        {
            return this.Run(buffer, 0, buffer.Length, testCount, expectedCount, millisecondsTimeout);
        }

        private async Task BeginSend()
        {
            while (!this.DisposedValue)
            {
                if (this.TryGet(out var byteBlocks))
                {
                    foreach (var block in byteBlocks)
                    {
                        block.SeekToStart();
                        try
                        {
                            var byteBlock = block;
                            while (byteBlock.CanRead)
                            {
                                if (this.m_adapter.TryParseRequest(ref byteBlock, out var request))
                                {
                                    this.m_count++;
                                    this.m_receivedCallBack?.Invoke(request);
                                }
                            }
                        }
                        catch(Exception ex)
                        { 
                        
                        }
                        finally
                        {
                            block.Dispose();
                        }
                    }
                }
                else
                {
                    await Task.Delay(1).ConfigureFalseAwait();
                }
            }
        }

        private Task SendCallback(ReadOnlyMemory<byte> memory)
        {
            var array = memory.ToArray();
            var asyncByte = new QueueDataBytes(array, 0, array.Length);
            this.m_asyncBytes.Enqueue(asyncByte);
            return EasyTask.CompletedTask;
        }

        private bool TryGet(out List<ByteBlock> byteBlocks)
        {
            byteBlocks = new List<ByteBlock>();
            ByteBlock block = null;
            while (true)
            {
                if (this.m_asyncBytes.TryDequeue(out var asyncByte))
                {
                    if (block == null)
                    {
                        block = BytePool.Default.GetByteBlock(this.m_bufferLength);
                        byteBlocks.Add(block);
                    }
                    var surLen = this.m_bufferLength - block.Position;
                    if (surLen < asyncByte.Length)//不能完成写入
                    {
                        block.Write(new ReadOnlySpan<byte>(asyncByte.Buffer, asyncByte.Offset, surLen));
                        var offset = surLen;
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
                        block.Write(new ReadOnlySpan<byte>(asyncByte.Buffer, asyncByte.Offset, asyncByte.Length));
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
            var block = BytePool.Default.GetByteBlock(this.m_bufferLength);
            var len = Math.Min(transferByte.Length - offset, this.m_bufferLength);
            block.Write(new ReadOnlySpan<byte>(transferByte.Buffer, offset, len));
            offset += len;

            return block;
        }
    }
}