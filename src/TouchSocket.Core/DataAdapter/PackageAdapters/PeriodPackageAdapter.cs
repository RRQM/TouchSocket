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

using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;

namespace TouchSocket.Core;

/// <summary>
/// 周期包适配
/// </summary>
public class PeriodPackageAdapter : SingleStreamDataHandlingAdapter
{
    private readonly ConcurrentQueue<ValueByteBlock> m_bytes = new ConcurrentQueue<ValueByteBlock>();
    private readonly CancellationTokenSource m_cts = new CancellationTokenSource();
    private readonly SemaphoreSlim m_semaphoreSlim = new SemaphoreSlim(1, 1);
    private int m_dataCount;
    private ExceptionDispatchInfo m_exceptionDispatchInfo;
    private long m_fireCount;

    /// <inheritdoc/>
    protected override Task PreviewReceivedAsync<TReader>(TReader reader)
    {
        this.m_exceptionDispatchInfo?.Throw();

        var dataLength = (int)reader.Sequence.Length;
        var valueByteBlock = new ValueByteBlock(dataLength);

        foreach (var item in reader.Sequence)
        {
            valueByteBlock.Write(item.Span);
            reader.Advance(item.Length);
        }

        this.m_bytes.Enqueue(valueByteBlock);
        Interlocked.Increment(ref this.m_fireCount);
        Interlocked.Add(ref this.m_dataCount, dataLength);

        this.ThrowIfMoreThanMaxPackageSize(this.m_dataCount);

        _ = EasyTask.SafeRun(this.DelayGo);
        return EasyTask.CompletedTask;
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            this.m_cts.SafeCancel();
            this.m_cts.SafeDispose();
        }
        base.SafetyDispose(disposing);
    }

    private async Task DelayGo()
    {
        await Task.Delay(this.CacheTimeout).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        if (Interlocked.Decrement(ref this.m_fireCount) == 0)
        {
            using (var byteBlock = new ValueByteBlock(this.m_dataCount))
            {
                while (this.m_bytes.TryDequeue(out var valueByteBlock))
                {
                    using (valueByteBlock)
                    {
                        byteBlock.Write(valueByteBlock.Span);
                        Interlocked.Add(ref this.m_dataCount, -valueByteBlock.Length);
                    }
                }

                byteBlock.SeekToStart();

                await this.m_semaphoreSlim.WaitAsync(this.m_cts.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                try
                {
                    await this.GoReceivedAsync(byteBlock.Memory, default).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
                catch (Exception ex)
                {
                    this.m_exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);
                }
                finally
                {
                    this.m_semaphoreSlim.Release();
                }
            }
        }
    }
}