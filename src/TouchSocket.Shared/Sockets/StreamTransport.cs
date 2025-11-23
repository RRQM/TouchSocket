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
using TouchSocket.Resources;

namespace TouchSocket.Sockets;

internal class StreamTransport : BaseTransport
{
    private readonly Stream m_stream;

    public StreamTransport(Stream stream, TransportOption option) : base(option)
    {
        this.m_stream = stream;

        this.Start();
    }

    public override async Task<Result> CloseAsync(string msg, CancellationToken cancellationToken = default)
    {
        try
        {
            await base.CloseAsync(msg, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_stream.Close();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex.Message);
        }
    }

    protected override async Task RunReceive(CancellationToken cancellationToken)
    {
        try
        {
            var pipeWriter = this.m_pipeReceive.Writer;
            while (!cancellationToken.IsCancellationRequested)
            {
                var memory = pipeWriter.GetMemory(this.ReceiveBufferSize);
                Debug.WriteLine($"StreamTransport RunReceive GetMemory Size:{memory.Length}");
                var result = await this.m_stream.ReadAsync(memory, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                if (result == 0)
                {
                    this.m_closedEventArgs ??= new ClosedEventArgs(false, TouchSocketResource.RemoteDisconnects);
                    await pipeWriter.CompleteAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    return;
                }

                this.m_receiveCounter.Increment(result);

                pipeWriter.Advance(result);
                var flushResult = await pipeWriter.FlushAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                if (flushResult.IsCompleted)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            this.m_closedEventArgs ??= new ClosedEventArgs(false, ex.Message);
            await this.m_pipeReceive.Writer.CompleteAsync(ex).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    protected override async Task RunSend(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var readResult = await this.m_pipeSend.Reader.ReadAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                if (readResult.IsCanceled)
                {
                    break;
                }

                if (readResult.IsCompleted && readResult.Buffer.IsEmpty)
                {
                    break;
                }

                try
                {
                    foreach (var item in readResult.Buffer)
                    {
                        await this.m_stream.WriteAsync(item, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }

                    this.m_pipeSend.Reader.AdvanceTo(readResult.Buffer.End);

                    this.m_sentCounter.Increment(readResult.Buffer.Length);
                }
                catch (Exception ex)
                {
                    // 处理发送失败的情况
                    this.m_pipeSend.Reader.Complete(ex);
                    break;
                }

                if (readResult.IsCompleted)
                {
                    break;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            this.m_pipeSend.Reader.Complete(ex);
        }
        finally
        {
            this.m_pipeSend.Reader.Complete();
        }
    }

    protected override void SafetyDispose(bool disposing)
    {
        this.m_stream.Dispose();
        base.SafetyDispose(disposing);
    }
}