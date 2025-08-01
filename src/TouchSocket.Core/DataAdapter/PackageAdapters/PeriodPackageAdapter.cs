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
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core;

/// <summary>
/// 周期包适配
/// </summary>
public class PeriodPackageAdapter : SingleStreamDataHandlingAdapter
{
    private readonly ConcurrentQueue<ValueByteBlock> m_bytes = new ConcurrentQueue<ValueByteBlock>();
    private long m_fireCount;
    private int m_dataCount;

    /// <inheritdoc/>
    protected override Task PreviewReceivedAsync(ByteBlock byteBlock)
    {
        var dataLength = byteBlock.Length;
        var valueByteBlock = new ValueByteBlock(dataLength);
        valueByteBlock.Write(byteBlock.Span);
        this.m_bytes.Enqueue(valueByteBlock);
        Interlocked.Increment(ref this.m_fireCount);
        Interlocked.Add(ref this.m_dataCount, dataLength);

        this.ThrowIfMoreThanMaxPackageSize(this.m_dataCount);

        _ = EasyTask.SafeRun(this.DelayGo);
        return EasyTask.CompletedTask;
    }

    private async Task DelayGo()
    {
        await Task.Delay(this.CacheTimeout).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        if (Interlocked.Decrement(ref this.m_fireCount) == 0)
        {
            using (var byteBlock = new ByteBlock(this.m_dataCount))
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

                try
                {
                    await this.GoReceivedAsync(byteBlock, default).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
                catch (Exception ex)
                {
                    this.OnError(ex, ex.Message, true, true);
                }
            }
        }
    }
}