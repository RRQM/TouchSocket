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
using System.Buffers;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.Http.WebSockets;

/// <summary>
/// 表示一个WebSocket客户端的设置配置对象。
/// </summary>
public abstract class SetupClientWebSocket : SetupConfigObject, IClosableClient, IOnlineClient
{
    /// <summary>
    /// 初始化 <see cref="SetupClientWebSocket"/> 类的新实例。
    /// </summary>
    public SetupClientWebSocket()
    {
        this.m_receiveCounter = new ValueCounter(TimeSpan.FromSeconds(1), this.OnReceivePeriod);
        this.m_sendCounter = new ValueCounter(TimeSpan.FromSeconds(1), this.OnSendPeriod);
    }

    #region 字段

    private readonly SemaphoreSlim m_semaphoreForConnect = new SemaphoreSlim(1, 1);
    private ClientWebSocket m_client;
    private Task m_runTask;
    private bool m_online;
    private ValueCounter m_receiveCounter;
    private ValueCounter m_sendCounter;
    private CancellationTokenSource m_tokenSourceForOnline;
    private ClosedEventArgs m_closedEventArgs;
    #endregion 字段

    #region 连接

    /// <inheritdoc/>
    public virtual async Task ConnectAsync(CancellationToken token)
    {
        this.ThrowIfDisposed();
        await this.m_semaphoreForConnect.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            if (this.m_online)
            {
                return;
            }

            if (this.m_client == null || this.m_client.State != WebSocketState.Open)
            {
                this.m_client.SafeDispose();
                this.m_client = new ClientWebSocket();
            }

            await this.m_client.ConnectAsync(this.RemoteIPHost, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            this.m_tokenSourceForOnline = new CancellationTokenSource();

            this.m_runTask = EasyTask.SafeRun(this.PrivateOnConnected, this.m_tokenSourceForOnline.Token);

            this.m_online = true;
        }
        finally
        {
            this.m_semaphoreForConnect.Release();
        }
    }

    private async Task PrivateOnConnected(CancellationToken token)
    {
        var receiveTask = EasyTask.SafeRun(this.ReceiveLoopAsync, token);

        await receiveTask.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        this.m_online = false;
        await this.OnWebSocketClosed(this.m_closedEventArgs).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private async Task WaitClearConnect()
    {
        // 确保上次接收任务已经结束
        var runTask = this.m_runTask;
        if (runTask != null)
        {
            await runTask.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }
    #endregion 连接

    /// <inheritdoc/>
    public DateTimeOffset LastReceivedTime => this.m_receiveCounter.LastIncrement;

    /// <inheritdoc/>
    public DateTimeOffset LastSentTime => this.m_sendCounter.LastIncrement;

    /// <inheritdoc/>
    public Protocol Protocol { get; set; } = Protocol.WebSocket;

    /// <inheritdoc/>
    public IPHost RemoteIPHost => this.Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);

    /// <summary>
    /// 通讯实际客户端
    /// </summary>
    protected ClientWebSocket ClientWebSocket => this.m_client;

    /// <inheritdoc/>
    public virtual bool Online => this.m_client != null && this.m_client.State == WebSocketState.Open && this.m_client.CloseStatus == null && this.m_online;

    /// <inheritdoc/>
    public CancellationToken ClosedToken => this.m_tokenSourceForOnline.GetTokenOrCanceled();

    /// <inheritdoc/>
    public virtual async Task<Result> CloseAsync(string msg, CancellationToken token = default)
    {
        try
        {
            if (!this.m_online)
            {
                return Result.Success;
            }

            this.m_closedEventArgs ??= new ClosedEventArgs(true, msg);
            await this.m_client.SafeCloseClientAsync(msg, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.CancelReceive();
            await this.WaitClearConnect().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }



    private void CancelReceive()
    {
        var tokenSourceForReceive = this.m_tokenSourceForOnline;
        if (tokenSourceForReceive != null)
        {
            tokenSourceForReceive.SafeCancel();
            tokenSourceForReceive.SafeDispose();
        }
        this.m_tokenSourceForOnline = null;
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            _ = EasyTask.SafeRun(async () => await this.CloseAsync(TouchSocketResource.DisposeClose).ConfigureAwait(EasyTask.ContinueOnCapturedContext));
        }

        base.SafetyDispose(disposing);
    }

    /// <summary>
    /// 已断开连接。
    /// </summary>
    /// <param name="e"></param>
    protected virtual Task OnWebSocketClosed(ClosedEventArgs e)
    {
        return EasyTask.CompletedTask;
    }

    protected abstract Task OnWebSocketReceived(WebSocketReceiveResult result, ReadOnlySequence<byte> sequence);

    /// <summary>
    /// 发送数据
    /// </summary>
    /// <param name="memory">数据</param>
    /// <param name="messageType"></param>
    /// <param name="endOfMessage"></param>
    /// <param name="token">可取消令箭</param>
    /// <returns></returns>
    protected async Task ProtectedSendAsync(ReadOnlyMemory<byte> memory, WebSocketMessageType messageType, bool endOfMessage, CancellationToken token)
    {
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        await this.m_client.SendAsync(memory, messageType, endOfMessage, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
#else
        var array = memory.GetArray();
        await this.m_client.SendAsync(array, messageType, endOfMessage, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
#endif

        this.m_sendCounter.Increment(memory.Length);
    }

    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        try
        {
            SegmentedBytesWriter writer = default;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var client = this.m_client;

                    if (client == null || client.State != WebSocketState.Open)
                    {
                        break;
                    }
                    writer ??= new SegmentedBytesWriter(1024 * 64);
                    var segment = writer.GetMemory(1024 * 64).GetArray();
                    var result = await client.ReceiveAsync(segment, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        try
                        {
                            if (!token.IsCancellationRequested)
                            {
                                await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                            }
                        }
                        catch
                        {
                        }
                        break;
                    }

                    if (result.Count == 0)
                    {
                        break;
                    }
                    this.m_receiveCounter.Increment(result.Count);
                    writer.Advance(result.Count);
                    if (result.EndOfMessage)
                    {
                        //处理数据
                        await this.OnWebSocketReceived(result, writer.Sequence).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        writer.Dispose();
                        writer = null;
                    }
                }
                catch (Exception ex)
                {
                    this.Logger?.Debug(this, ex);
                    break;
                }
            }
            this.m_closedEventArgs ??= new ClosedEventArgs(false, TouchSocketResource.RemoteDisconnects);
        }
        catch (Exception ex)
        {
            this.m_closedEventArgs = new ClosedEventArgs(false, ex.Message);
            this.Logger?.Debug(this, ex);
        }
    }

    private void OnReceivePeriod(long value)
    {
        //this.m_receiveBufferSize = TouchSocketCoreUtility.HitBufferLength(value);
    }

    private void OnSendPeriod(long value)
    {
        //this.m_sendBufferSize = TouchSocketCoreUtility.HitBufferLength(value);
    }
}