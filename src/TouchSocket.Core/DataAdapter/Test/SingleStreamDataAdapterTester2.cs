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

using System.Diagnostics;
using System.IO.Pipelines;

namespace TouchSocket.Core;

/// <summary>
/// 单线程状况的流式数据处理适配器测试。
/// </summary>
/// <typeparam name="TAdapter">自定义数据处理适配器类型。</typeparam>
/// <typeparam name="TRequest">请求信息类型。</typeparam>
public class SingleStreamDataAdapterTester<TAdapter, TRequest>
    where TAdapter : CustomDataHandlingAdapter<TRequest>
    where TRequest : class, IRequestInfo
{
    private readonly TAdapter m_adapter;
    private readonly Pipe m_pipe = new Pipe(new PipeOptions(default, default, default, 1024 * 1024 * 1024, 1024 * 1024 * 512, -1));
    private readonly Action<TRequest> m_receivedCallBack;
    private int m_bufferLength;
    private int m_count;
    private int m_expectedCount;

    /// <summary>
    /// Tcp数据处理适配器测试
    /// </summary>
    /// <param name="adapter">自定义数据处理适配器实例。</param>
    /// <param name="receivedCallBack">接收回调委托。</param>
    public SingleStreamDataAdapterTester(TAdapter adapter, Action<TRequest> receivedCallBack = default)
    {
        this.m_adapter = adapter;
        this.m_receivedCallBack = receivedCallBack;
    }

    /// <summary>
    /// 异步运行测试。
    /// </summary>
    /// <param name="memory">要发送的数据内存块。</param>
    /// <param name="testCount">测试发送次数。</param>
    /// <param name="expectedCount">预期接收次数。</param>
    /// <param name="bufferLength">每次写入的缓冲区长度。</param>
    /// <param name="cancellationToken">取消操作的令牌。</param>
    /// <returns>返回测试所用的时间。</returns>
    public async Task<TimeSpan> RunAsync(ReadOnlyMemory<byte> memory, int testCount, int expectedCount, int bufferLength, CancellationToken cancellationToken)
    {
        this.m_count = 0;
        this.m_expectedCount = expectedCount;
        this.m_bufferLength = bufferLength;
        var receivedTask = EasyTask.SafeRun(this.ReceivedLoopAsync, cancellationToken);

        var m_stopwatch = new Stopwatch();
        m_stopwatch.Start();

        for (var i = 0; i < testCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var valueByteBlock = new ValueByteBlock(memory.Length + 1024);
            try
            {
                this.m_adapter.SendInput(ref valueByteBlock, memory);

                await this.SendCallback(valueByteBlock.Memory, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            finally
            {
                valueByteBlock.Dispose();
            }
        }

        await this.m_pipe.Writer.CompleteAsync();

        await receivedTask.WithCancellation(cancellationToken);

        if (this.m_count != this.m_expectedCount)
        {
            throw new Exception($"实际接收次数：{this.m_count}，预期接收次数：{this.m_expectedCount}");
        }

        m_stopwatch.Stop();

        await this.m_pipe.Reader.CompleteAsync();
        this.m_pipe.Reset();

        var elapsed = m_stopwatch.Elapsed;
        return elapsed;
    }

    /// <summary>
    /// 以指定的超时时间异步运行测试。
    /// </summary>
    /// <param name="memory">要发送的数据内存块。</param>
    /// <param name="testCount">测试发送次数。</param>
    /// <param name="expectedCount">预期接收次数。</param>
    /// <param name="bufferLength">每次写入的缓冲区长度。</param>
    /// <param name="millisecondsTimeout">超时时间（毫秒）。</param>
    /// <returns>返回测试所用的时间。</returns>
    /// <exception cref="TimeoutException">超时未完成时抛出。</exception>
    public async Task<TimeSpan> RunAsync(ReadOnlyMemory<byte> memory, int testCount, int expectedCount, int bufferLength, int millisecondsTimeout)
    {
        using (var cancellationToken = new CancellationTokenSource(millisecondsTimeout))
        {
            try
            {
                return await this.RunAsync(memory, testCount, expectedCount, bufferLength, cancellationToken.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    private async Task ReceivedLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var readResult = await this.m_pipe.Reader.ReadAsync(cancellationToken);

            var reader = new BytesReader(readResult.Buffer);

            while (reader.BytesRemaining > 0)
            {
                if (!this.m_adapter.TryParseRequest(ref reader, out var request))
                {
                    break;
                }

                this.m_count++;
                this.m_receivedCallBack?.Invoke(request);
            }

            var position = readResult.Buffer.GetPosition(reader.BytesRead);
            this.m_pipe.Reader.AdvanceTo(position, readResult.Buffer.End);

            if (readResult.IsCanceled || readResult.IsCompleted)
            {
                return;
            }
        }
    }

    private async Task SendCallback(ReadOnlyMemory<byte> memory, CancellationToken cancellationToken)
    {
        var offset = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var remainingLength = memory.Length - offset;
            if (remainingLength <= 0)
            {
                break;
            }
            var sliceMemory = memory.Slice(offset, Math.Min(remainingLength, this.m_bufferLength));
            await this.m_pipe.Writer.WriteAsync(sliceMemory, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            offset += sliceMemory.Length;
        }
    }
}