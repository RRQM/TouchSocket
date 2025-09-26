// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System.Diagnostics;
using System.Net;

namespace TouchSocket.Core;

/// <summary>
/// 多线程数据适配器测试器，用于在多线程环境下测试 <see cref="UdpDataHandlingAdapter"/> 的性能和正确性。
/// 该类提供了高并发场景下的数据处理适配器测试功能，可以模拟多线程发送和接收数据。
/// </summary>
/// <remarks>
/// 测试器通过内部智能队列管理数据传输，支持异步操作和性能统计。
/// 可用于验证数据适配器在高负载情况下的稳定性和性能表现。
/// </remarks>
public class MultithreadingDataAdapterTester : SafetyDisposableObject
{
    private readonly IntelligentDataQueue<QueueDataBytes> m_asyncBytes;

    private UdpDataHandlingAdapter m_adapter;

    private volatile int m_count;

    private int m_expectedCount;

    private int m_millisecondsTimeout;

    private Func<ReadOnlyMemory<byte>, IRequestInfo, Task> m_receivedCallBack;

    private Stopwatch m_stopwatch;

    /// <summary>
    /// 初始化 <see cref="MultithreadingDataAdapterTester"/> 类的新实例。
    /// </summary>
    /// <param name="multiThread">并发多线程数量，用于确定同时处理数据的线程数。</param>
    protected MultithreadingDataAdapterTester(int multiThread)
    {
        this.m_asyncBytes = new IntelligentDataQueue<QueueDataBytes>(1024 * 1024 * 10);
        for (var i = 0; i < multiThread; i++)
        {
            _ = EasyTask.SafeRun(this.BeginSend);
        }
    }

    /// <summary>
    /// 创建数据适配器测试器实例。
    /// </summary>
    /// <param name="adapter">待测试的 <see cref="UdpDataHandlingAdapter"/> 适配器。</param>
    /// <param name="multiThread">并发多线程数量，决定测试的并发程度。</param>
    /// <param name="receivedCallBack">收到数据时的回调函数，可选参数。如果为 <see langword="null"/>，则不执行额外的回调处理。</param>
    /// <returns>返回配置完成的 <see cref="MultithreadingDataAdapterTester"/> 实例。</returns>
    /// <remarks>
    /// 该方法会自动配置适配器的发送和接收回调函数，建立测试器与适配器之间的数据流转机制。
    /// </remarks>
    public static MultithreadingDataAdapterTester CreateTester(UdpDataHandlingAdapter adapter, int multiThread, Func<ReadOnlyMemory<byte>, IRequestInfo, Task> receivedCallBack = default)
    {
        var tester = new MultithreadingDataAdapterTester(multiThread);
        tester.m_adapter = adapter;
        adapter.SendCallBackAsync = tester.SendCallback;
        adapter.ReceivedCallBack = tester.OnReceived;
        tester.m_receivedCallBack = receivedCallBack;
        return tester;
    }

    /// <summary>
    /// 异步执行模拟测试运行，发送指定的数据并统计处理时间。
    /// </summary>
    /// <param name="memory">待测试的内存数据块，包含要发送的字节数据。</param>
    /// <param name="testCount">测试发送的总次数。</param>
    /// <param name="expectedCount">期望接收到的数据包数量，用于判断测试是否完成。</param>
    /// <param name="millisecondsTimeout">测试超时时间，单位为毫秒。如果在指定时间内未完成测试，将抛出超时异常。</param>
    /// <returns>返回一个 <see cref="Task{TimeSpan}"/>，表示测试完成所用的时间。</returns>
    /// <exception cref="TimeoutException">当测试在指定超时时间内未完成时抛出。</exception>
    /// <remarks>
    /// 该方法会启动计时器，发送指定次数的数据包，然后等待接收到期望数量的响应。
    /// 测试过程是异步的，支持高并发数据发送和处理。
    /// </remarks>
    public async Task<TimeSpan> RunAsync(ReadOnlyMemory<byte> memory, int testCount, int expectedCount, int millisecondsTimeout)
    {
        this.m_count = 0;
        this.m_expectedCount = expectedCount;
        this.m_millisecondsTimeout = millisecondsTimeout;
        this.m_stopwatch = new Stopwatch();
        this.m_stopwatch.Start();
        for (var i = 0; i < testCount; i++)
        {
            await this.m_adapter.SendInputAsync(null, memory, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        if (SpinWait.SpinUntil(() => this.m_count == this.m_expectedCount, this.m_millisecondsTimeout))
        {
            this.m_stopwatch.Stop();
            return this.m_stopwatch.Elapsed;
        }

        throw new TimeoutException();
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
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
                        await this.m_adapter.ReceivedInputAsync(null, block.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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

    private async Task OnReceived(EndPoint endPoint, ReadOnlyMemory<byte> memory, IRequestInfo requestInfo)
    {
        if (this.m_receivedCallBack != null)
        {
            await this.m_receivedCallBack(memory, requestInfo).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        Interlocked.Increment(ref this.m_count);
    }

    private Task SendCallback(EndPoint endPoint, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken)
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