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

using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Mqtt;

public sealed class MqttClientActor : MqttActor
{
    private TaskCompletionSource<MqttConnAckMessage> m_waitForConnect;
    private readonly WaitDataAsync<MqttPingRespMessage> m_waitForPing = new();

    public async Task DisconnectAsync(CancellationToken token)
    {
        if (!this.Online)
        {
            return;
        }
        try
        {
            await this.ProtectedOutputSendAsync(new MqttDisconnectMessage(), token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch
        {
        }
        finally
        {
            this.Online = false;
        }
    }

    public async ValueTask<Result> PingAsync(CancellationToken token)
    {
        var contentForAck = new MqttPingReqMessage();
        this.m_waitForPing.Reset();
        await this.ProtectedOutputSendAsync(contentForAck, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        var waitDataStatus = await this.m_waitForPing.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

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

    #region 连接

    public async Task<MqttConnAckMessage> ConnectAsync(MqttConnectMessage message, CancellationToken token)
    {
        this.m_waitForConnect = new TaskCompletionSource<MqttConnAckMessage>();

        await this.ProtectedOutputSendAsync(message, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        var connAckMessage = await this.m_waitForConnect.Task.WithCancellation(token);
        this.Online = true;
        return connAckMessage;
    }

    #endregion 连接

    #region 重写

    protected override Task InputMqttConnAckMessageAsync(MqttConnAckMessage message, CancellationToken token)
    {
        this.m_waitForConnect?.SetResult(message);
        return EasyTask.CompletedTask;
    }

    protected override Task InputMqttConnectMessageAsync(MqttConnectMessage message, CancellationToken token)
    {
        throw ThrowHelper.CreateNotSupportedException($"遇到无法处理的数据报文,Message={message}");
    }

    protected override Task InputMqttPingRespMessageAsync(MqttPingRespMessage message, CancellationToken token)
    {
        this.m_waitForPing.Set(message);
        return EasyTask.CompletedTask;
    }

    protected override Task InputMqttSubscribeMessageAsync(MqttSubscribeMessage message, CancellationToken token)
    {
        throw ThrowHelper.CreateNotSupportedException($"遇到无法处理的数据报文,Message={message}");
    }

    protected override Task InputMqttUnsubscribeMessageAsync(MqttUnsubscribeMessage message, CancellationToken token)
    {
        throw ThrowHelper.CreateNotSupportedException($"遇到无法处理的数据报文,Message={message}");
    }

    #endregion 重写

    public async Task<MqttSubAckMessage> SubscribeAsync(MqttSubscribeMessage message, CancellationToken token = default)
    {
        using (var waitDataAsync = this.WaitHandlePool.GetWaitDataAsync(message))
        {
            await this.ProtectedOutputSendAsync(message, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            var waitDataStatus = await waitDataAsync.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            waitDataStatus.ThrowIfNotRunning();
            var subAckMessage = (MqttSubAckMessage)waitDataAsync.CompletedData;
            return subAckMessage;
        }
    }

    public async Task<MqttUnsubAckMessage> UnsubscribeAsync(MqttUnsubscribeMessage message, CancellationToken token = default)
    {
        using (var waitDataAsync = this.WaitHandlePool.GetWaitDataAsync(message))
        {
            await this.ProtectedOutputSendAsync(message, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            var waitDataStatus = await waitDataAsync.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            waitDataStatus.ThrowIfNotRunning();
            var subAckMessage = (MqttUnsubAckMessage)waitDataAsync.CompletedData;
            return subAckMessage;
        }
    }
}