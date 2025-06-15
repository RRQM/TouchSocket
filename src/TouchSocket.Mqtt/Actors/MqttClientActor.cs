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

public class MqttClientActor : MqttActor
{
    private readonly WaitDataAsync<MqttConnAckMessage> m_waitForConnect = new();
    private readonly WaitDataAsync<MqttPingRespMessage> m_waitForPing = new();

    public async Task DisconnectAsync()
    {
        if (!this.Online)
        {
            return;
        }
        try
        {
            await this.ProtectedOutputSendAsync(new MqttDisconnectMessage()).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch
        {
        }
        finally
        {
            this.Online = false;
        }
    }

    public async Task PingAsync(int timeout, CancellationToken token)
    {
        var contentForAck = new MqttPingReqMessage();
        this.m_waitForPing.Reset();
        this.m_waitForPing.SetCancellationToken(token);
        await this.ProtectedOutputSendAsync(contentForAck);
        var waitDataStatus = await this.m_waitForPing.WaitAsync(timeout).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        waitDataStatus.ThrowIfNotRunning();
    }

    #region 连接

    public async Task<MqttConnAckMessage> ConnectAsync(MqttConnectMessage message, int millisecondsTimeout, CancellationToken token)
    {
        this.m_waitForConnect.Reset();
        this.m_waitForConnect.SetCancellationToken(token);

        await this.ProtectedOutputSendAsync(message).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        var waitDataStatus = await this.m_waitForConnect.WaitAsync(millisecondsTimeout);
        waitDataStatus.ThrowIfNotRunning();
        this.Online = true;
        return this.m_waitForConnect.WaitResult;
    }

    #endregion 连接

    #region 重写

    protected override Task InputMqttConnAckMessageAsync(MqttConnAckMessage message)
    {
        this.m_waitForConnect.Set(message);
        return EasyTask.CompletedTask;
    }

    protected override Task InputMqttConnectMessageAsync(MqttConnectMessage message)
    {
        throw ThrowHelper.CreateNotSupportedException($"遇到无法处理的数据报文,Message={message}");
    }

    protected override Task InputMqttPingRespMessageAsync(MqttPingRespMessage message)
    {
        this.m_waitForPing.Set(message);
        return EasyTask.CompletedTask;
    }

    protected override Task InputMqttSubscribeMessageAsync(MqttSubscribeMessage message)
    {
        throw ThrowHelper.CreateNotSupportedException($"遇到无法处理的数据报文,Message={message}");
    }

    protected override Task InputMqttUnsubscribeMessageAsync(MqttUnsubscribeMessage message)
    {
        throw ThrowHelper.CreateNotSupportedException($"遇到无法处理的数据报文,Message={message}");
    }

    #endregion 重写

    public async Task<MqttSubAckMessage> SubscribeAsync(MqttSubscribeMessage message, int timeout = 5000, CancellationToken token = default)
    {
        var waitDataAsync = this.WaitHandlePool.GetWaitDataAsync(message);

        try
        {
            waitDataAsync.SetCancellationToken(token);

            await this.ProtectedOutputSendAsync(message).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            var waitDataStatus = await waitDataAsync.WaitAsync(timeout).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            waitDataStatus.ThrowIfNotRunning();
            var subAckMessage = (MqttSubAckMessage)waitDataAsync.WaitResult;
            return subAckMessage;
        }
        finally
        {
            this.WaitHandlePool.Destroy(message.MessageId);
        }

    }

    public async Task<MqttUnsubAckMessage> UnsubscribeAsync(MqttUnsubscribeMessage message, int timeout = 5000, CancellationToken token = default)
    {
        var waitDataAsync = this.WaitHandlePool.GetWaitDataAsync(message);

        try
        {
            waitDataAsync.SetCancellationToken(token);

            await this.ProtectedOutputSendAsync(message).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            var waitDataStatus = await waitDataAsync.WaitAsync(timeout).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            waitDataStatus.ThrowIfNotRunning();
            var subAckMessage = (MqttUnsubAckMessage)waitDataAsync.WaitResult;
            return subAckMessage;
        }
        finally
        {
            this.WaitHandlePool.Destroy(message.MessageId);
        }

    }
}