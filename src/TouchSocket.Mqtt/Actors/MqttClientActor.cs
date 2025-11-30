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

namespace TouchSocket.Mqtt;

public sealed class MqttClientActor : MqttActor
{
    private TaskCompletionSource<MqttConnAckMessage> m_waitForConnect;
    private readonly WaitDataAsync<MqttPingRespMessage> m_waitForPing = new();

    /// <summary>
    /// 异步断开连接。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>任务。</returns>
    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        if (!this.Online)
        {
            return;
        }
        try
        {
            await this.ProtectedOutputSendAsync(new MqttDisconnectMessage(), cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch
        {
        }
        finally
        {
            this.Online = false;
        }
    }

    /// <summary>
    /// 异步发送PING消息。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>操作结果。</returns>
    public async ValueTask<Result> PingAsync(CancellationToken cancellationToken)
    {
        var contentForAck = new MqttPingReqMessage();
        this.m_waitForPing.Reset();
        await this.ProtectedOutputSendAsync(contentForAck, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        var waitDataStatus = await this.m_waitForPing.WaitAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        return waitDataStatus switch
        {
            WaitDataStatus.Default => Result.UnknownFail,
            WaitDataStatus.Success => Result.Success,
            WaitDataStatus.Overtime => Result.Overtime,
            WaitDataStatus.Canceled => Result.Canceled,
            WaitDataStatus.Disposed => Result.Disposed,
            _ => Result.UnknownFail,
        };
    }

    /// <summary>
    /// 异步连接。
    /// </summary>
    /// <param name="message">连接消息。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>连接确认消息。</returns>
    public async Task<MqttConnAckMessage> ConnectAsync(MqttConnectMessage message, CancellationToken cancellationToken)
    {
        this.m_waitForConnect = new TaskCompletionSource<MqttConnAckMessage>();

        await this.ProtectedOutputSendAsync(message, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        var connAckMessage = await this.m_waitForConnect.Task.WithCancellation(cancellationToken);
        this.Online = true;
        return connAckMessage;
    }

    #region 重写

    /// <inheritdoc/>
    protected override Task InputMqttConnAckMessageAsync(MqttConnAckMessage message, CancellationToken cancellationToken)
    {
        this.m_waitForConnect?.SetResult(message);
        return EasyTask.CompletedTask;
    }

    /// <inheritdoc/>
    protected override Task InputMqttConnectMessageAsync(MqttConnectMessage message, CancellationToken cancellationToken)
    {
        throw ThrowHelper.CreateNotSupportedException($"遇到无法处理的数据报文,Message={message}");
    }

    /// <inheritdoc/>
    protected override Task InputMqttPingRespMessageAsync(MqttPingRespMessage message, CancellationToken cancellationToken)
    {
        this.m_waitForPing.Set(message);
        return EasyTask.CompletedTask;
    }

    /// <inheritdoc/>
    protected override Task InputMqttSubscribeMessageAsync(MqttSubscribeMessage message, CancellationToken cancellationToken)
    {
        throw ThrowHelper.CreateNotSupportedException($"遇到无法处理的数据报文,Message={message}");
    }

    /// <inheritdoc/>
    protected override Task InputMqttUnsubscribeMessageAsync(MqttUnsubscribeMessage message, CancellationToken cancellationToken)
    {
        throw ThrowHelper.CreateNotSupportedException($"遇到无法处理的数据报文,Message={message}");
    }

    #endregion 重写

    /// <summary>
    /// 异步订阅。
    /// </summary>
    /// <param name="message">订阅消息。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>订阅确认消息。</returns>
    public async Task<MqttSubAckMessage> SubscribeAsync(MqttSubscribeMessage message, CancellationToken cancellationToken = default)
    {
        using (var waitDataAsync = this.WaitHandlePool.GetWaitDataAsync(message))
        {
            await this.ProtectedOutputSendAsync(message, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            var waitDataStatus = await waitDataAsync.WaitAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            waitDataStatus.ThrowIfNotRunning();
            var subAckMessage = (MqttSubAckMessage)waitDataAsync.CompletedData;
            return subAckMessage;
        }
    }

    /// <summary>
    /// 异步取消订阅。
    /// </summary>
    /// <param name="message">取消订阅消息。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>取消订阅确认消息。</returns>
    public async Task<MqttUnsubAckMessage> UnsubscribeAsync(MqttUnsubscribeMessage message, CancellationToken cancellationToken = default)
    {
        using (var waitDataAsync = this.WaitHandlePool.GetWaitDataAsync(message))
        {
            await this.ProtectedOutputSendAsync(message, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            var waitDataStatus = await waitDataAsync.WaitAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            waitDataStatus.ThrowIfNotRunning();
            var subAckMessage = (MqttUnsubAckMessage)waitDataAsync.CompletedData;
            return subAckMessage;
        }
    }
}