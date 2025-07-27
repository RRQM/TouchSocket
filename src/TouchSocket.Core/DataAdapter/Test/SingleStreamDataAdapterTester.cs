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

namespace TouchSocket.Core;

/// <summary>
///单线程状况的流式数据处理适配器测试
/// </summary>
public class SingleStreamDataAdapterTester : DisposableObject
{
    private readonly IntelligentDataQueue<QueueDataBytes> m_asyncBytes;
    private SingleStreamDataHandlingAdapter m_adapter;
    private int m_bufferLength;
    private int m_count;
    private int m_expectedCount;
    private Func<IByteBlockReader, IRequestInfo, Task> m_receivedCallBack;
    private Stopwatch m_stopwatch;
    private int m_timeout;

    /// <summary>
    /// Tcp数据处理适配器测试
    /// </summary>
    protected SingleStreamDataAdapterTester()
    {
        this.m_asyncBytes = new IntelligentDataQueue<QueueDataBytes>(1024 * 1024 * 10);

        _ = EasyTask.SafeRun(this.BeginSend);
    }

    /// <summary>
    /// 获取测试器
    /// </summary>
    /// <param name="adapter">待测试适配器</param>
    /// <param name="receivedCallBack">收到数据回调</param>
    /// <param name="bufferLength">缓存数据长度</param>
    /// <returns></returns>
    public static SingleStreamDataAdapterTester CreateTester(SingleStreamDataHandlingAdapter adapter, int bufferLength = 1024, Func<IByteBlockReader, IRequestInfo, Task> receivedCallBack = default)
    {
        var tester = new SingleStreamDataAdapterTester
        {
            m_adapter = adapter,
            m_bufferLength = bufferLength
        };
        adapter.SendAsyncCallBack = tester.SendCallback;
        adapter.ReceivedAsyncCallBack = tester.OnReceived;
        tester.m_receivedCallBack = receivedCallBack;
        return tester;
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
        _ = EasyTask.SafeRun(async () =>
        {
            for (var i = 0; i < testCount; i++)
            {
                await this.m_adapter.SendInputAsync(new Memory<byte>(buffer, offset, length), CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
                    try
                    {
                        block.SeekToStart();
                        await this.m_adapter.ReceivedInputAsync(block).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }
                    finally
                    {
                        block.Dispose();
                    }
                }
            }
            else
            {
                await Task.Delay(1).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
    }

    private async Task OnReceived(IByteBlockReader byteBlock, IRequestInfo requestInfo)
    {
        this.m_count++;
        if (this.m_receivedCallBack != null)
        {
            await this.m_receivedCallBack.Invoke(byteBlock, requestInfo).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    private Task SendCallback(ReadOnlyMemory<byte> memory, CancellationToken token)
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
                    block = new ByteBlock(this.m_bufferLength);
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
        var block = new ByteBlock(this.m_bufferLength);
        var len = Math.Min(transferByte.Length - offset, this.m_bufferLength);
        block.Write(new ReadOnlySpan<byte>(transferByte.Buffer, offset, len));
        offset += len;

        return block;
    }
}