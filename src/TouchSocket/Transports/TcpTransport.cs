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

using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets;

internal sealed class TcpTransport : BaseTransport
{
    private readonly TcpCore m_tcpCore;

    public TcpTransport(TcpCore tcpCore, TransportOption option) : base(option)
    {
        this.m_tcpCore = tcpCore ?? throw new ArgumentNullException(nameof(tcpCore));

        this.Start();
    }

    public override async Task<Result> CloseAsync(string msg, CancellationToken token = default)
    {
        try
        {
            await base.CloseAsync(msg, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_tcpCore.Close();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex.Message);
        }
    }

    protected override async Task RunReceive(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var memory = this.m_pipeReceive.Writer.GetMemory(this.ReceiveBufferSize);
                var result = await this.m_tcpCore.ReceiveAsync(memory, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                if (result.SocketError == SocketError.Success)
                {
                    if (result.BytesTransferred == 0)
                    {
                        this.ClosedEventArgs ??= new ClosedEventArgs(false, TouchSocketResource.RemoteDisconnects);
                        await this.m_pipeReceive.Writer.CompleteAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        return;
                    }

                    this.m_receiveCounter.Increment(result.BytesTransferred);

                    this.m_pipeReceive.Writer.Advance(result.BytesTransferred);
                    var flushResult = await this.m_pipeReceive.Writer.FlushAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    if (flushResult.IsCompleted)
                    {
                        break;
                    }
                }
                else
                {
                    // 处理接收失败的情况
                    this.ClosedEventArgs ??= new ClosedEventArgs(false, TouchSocketResource.RemoteDisconnects);
                    await this.m_pipeReceive.Writer.CompleteAsync(new SocketException((int)result.SocketError)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            this.ClosedEventArgs ??= new ClosedEventArgs(false, TouchSocketResource.RemoteDisconnects);
            await this.m_pipeReceive.Writer.CompleteAsync(ex).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    protected override async Task RunSend(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var readResult = await this.m_pipeSend.Reader.ReadAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
                    await this.m_tcpCore.SendAsync(readResult.Buffer, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
}