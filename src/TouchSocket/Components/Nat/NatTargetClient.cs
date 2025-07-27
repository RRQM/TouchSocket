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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// 封装类，用于处理NAT穿透后的目标客户端连接。
/// </summary>
/// <remarks>
/// 该类继承自TcpClientBase，并实现了ITcpConnectableClient和IClientSender接口，
/// 以支持TCP连接和客户端数据发送功能。
/// </remarks>
public sealed class NatTargetClient : TcpClientBase, ITcpConnectableClient, IClientSender
{
    internal Func<ITcpSession, ClosedEventArgs, Task> m_internalClosed;

    internal Func<NatTargetClient, ReceivedDataEventArgs, Task> m_internalReceived;

    /// <summary>
    /// 初始化 NatTargetClient 类的新实例。
    /// </summary>
    public NatTargetClient()
    {
    }

    /// <summary>
    /// 初始化 NatTargetClient 类的新实例，并设置是否为备用模式。
    /// </summary>
    /// <param name="standBy">指示是否将客户端置于备用模式。</param>
    public NatTargetClient(bool standBy)
    {
        this.StandBy = standBy;
    }

    /// <summary>
    /// 是否独立化当前对象。当为<see langword="true"/>时，<see cref="NatSessionClient"/>即使断线，也不会释放该对象。
    /// </summary>
    public bool StandBy { get; }

    /// <inheritdoc/>
    public Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
    {
        return base.TcpConnectAsync(millisecondsTimeout, token);
    }

    /// <inheritdoc/>
    public Task SendAsync(ReadOnlyMemory<byte> memory,CancellationToken token=default)
    {
        return base.ProtectedSendAsync(memory, token);
    }

    /// <inheritdoc/>
    public Task SendAsync(IRequestInfo requestInfo, CancellationToken token = default)
    {
        return this.ProtectedSendAsync(requestInfo, token);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosed(ClosedEventArgs e)
    {
        if (this.m_internalClosed != null)
        {
            await this.m_internalClosed.Invoke(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        await base.OnTcpClosed(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        if (this.m_internalReceived != null)
        {
            await this.m_internalReceived.Invoke(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        await base.OnTcpReceived(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }
}