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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// 定义了一个抽象类TcpSessionClient，用于处理TCP会话客户端的连接和数据传输。
/// 它继承自TcpSessionClientBase类，并实现了ITcpSessionClient接口。
///
/// 该类提供了基础的TCP会话管理功能，包括客户端的标识(Id)、IP地址(IP)和端口号(Port)。
/// 使用DebuggerDisplay属性，可以在调试工具中更清晰地展示每个实例的Id、IP地址和端口号。
///
/// 继承此类的子类通常需要实现或重写一些方法和属性，以适应特定的业务逻辑和数据处理需求。
/// </summary>
[DebuggerDisplay("Id={Id},IPAddress={IP}:{Port}")]
public abstract class TcpSessionClient : TcpSessionClientBase, ITcpSessionClient
{
    #region 变量

    private Func<TcpSessionClient, ConnectedEventArgs, Task> m_onClientConnected;
    private Func<TcpSessionClient, ConnectingEventArgs, Task> m_onClientConnecting;
    private Func<TcpSessionClient, ClosedEventArgs, Task> m_onClientDisconnected;
    private Func<TcpSessionClient, ClosingEventArgs, Task> m_onClientDisconnecting;
    private Func<TcpSessionClient, ReceivedDataEventArgs, Task> m_onClientReceivedData;

    #endregion 变量

    internal void SetAction(Func<TcpSessionClient, ConnectingEventArgs, Task> onClientConnecting, Func<TcpSessionClient, ConnectedEventArgs, Task> onClientConnected, Func<TcpSessionClient, ClosingEventArgs, Task> onClientDisconnecting, Func<TcpSessionClient, ClosedEventArgs, Task> onClientDisconnected, Func<TcpSessionClient, ReceivedDataEventArgs, Task> onClientReceivedData)
    {
        this.m_onClientConnecting = onClientConnecting;
        this.m_onClientConnected = onClientConnected;
        this.m_onClientDisconnecting = onClientDisconnecting;
        this.m_onClientDisconnected = onClientDisconnected;
        this.m_onClientReceivedData = onClientReceivedData;
    }

    #region 事件&委托

    /// <inheritdoc/>
    public ClosedEventHandler<ITcpSessionClient> Closed { get; set; }

    /// <inheritdoc/>
    public ClosingEventHandler<ITcpSessionClient> Closing { get; set; }

    /// <summary>
    /// 客户端已断开连接。
    /// </summary>
    /// <param name="e">有关断开连接事件的信息。</param>
    protected override async Task OnTcpClosed(ClosedEventArgs e)
    {
        // 如果已注册Closed事件处理程序，则调用该处理程序
        if (this.Closed != null)
        {
            // 异步调用Closed事件处理程序，并等待它完成
            await this.Closed.Invoke(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            // 如果事件已被处理，则直接返回
            if (e.Handled)
            {
                return;
            }
        }

        // 调用自定义的客户端断开连接处理逻辑
        if (this.m_onClientDisconnected != null)
        {
            await this.m_onClientDisconnected(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            // 再次检查事件是否已被处理
            if (e.Handled)
            {
                return;
            }
        }

        // 调用基类的OnTcpClosed方法，传递事件参数
        await base.OnTcpClosed(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 即将断开连接(仅主动断开时有效)。
    /// </summary>
    /// <param name="e">有关断开连接的事件参数</param>
    protected override async Task OnTcpClosing(ClosingEventArgs e)
    {
        // 如果已注册断开连接事件处理程序
        if (this.Closing != null)
        {
            // 调用注册的事件处理程序
            await this.Closing.Invoke(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            // 如果事件已被处理，则直接返回
            if (e.Handled)
            {
                return;
            }
        }

        // 调用内部的客户端断开连接处理方法
        if (this.m_onClientDisconnecting != null)
        {
            await this.m_onClientDisconnecting(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            // 再次检查事件是否已被处理
            if (e.Handled)
            {
                return;
            }
        }


        // 调用基类的即将断开连接方法
        await base.OnTcpClosing(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 当客户端完整建立Tcp连接。
    /// </summary>
    /// <param name="e">包含连接建立信息的事件参数。</param>
    protected override async Task OnTcpConnected(ConnectedEventArgs e)
    {
        // 触发客户端连接事件，允许派生类处理连接逻辑。
        if (this.m_onClientConnected != null)
        {
            await this.m_onClientConnected(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            // 再次检查事件是否已被处理
            if (e.Handled)
            {
                return;
            }
        }

        // 执行基类的OnTcpConnected方法，继续默认的连接处理流程。
        await base.OnTcpConnected(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 客户端正在连接。
    /// </summary>
    /// <param name="e">包含连接信息的事件参数。</param>
    /// <returns>一个等待的任务，该任务在连接完成后会被完成。</returns>
    protected override async Task OnTcpConnecting(ConnectingEventArgs e)
    {
        // 触发客户端连接事件，允许派生类处理连接逻辑
        if (this.m_onClientConnecting != null)
        {
            await this.m_onClientConnecting(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            // 如果事件已经被处理，则不再继续父类的连接逻辑
            if (e.Handled)
            {
                return;
            }
        }

        // 调用基类的连接逻辑继续处理连接
        await base.OnTcpConnecting(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion 事件&委托

    /// <summary>
    /// 当收到适配器处理的数据时。
    /// </summary>
    /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        // 调用注册的事件处理程序来处理接收到的数据
        if (this.m_onClientReceivedData != null)
        {
            await this.m_onClientReceivedData(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            // 如果数据已经被处理，则不再向下传递
            if (e.Handled)
            {
                return;
            }
        }

        // 调用基类的相应方法继续处理数据
        await base.OnTcpReceived(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #region 异步发送

    /// <inheritdoc/>
    public virtual Task SendAsync(ReadOnlyMemory<byte> memory)
    {
        return this.ProtectedSendAsync(memory);
    }

    /// <inheritdoc/>
    public virtual Task SendAsync(IRequestInfo requestInfo)
    {
        return this.ProtectedSendAsync(requestInfo);
    }

    /// <inheritdoc/>
    public virtual Task SendAsync(IList<ArraySegment<byte>> transferBytes)
    {
        return this.ProtectedSendAsync(transferBytes);
    }

    #endregion 异步发送

    #region Id发送

    /// <inheritdoc/>
    public Task SendAsync(string id, ReadOnlyMemory<byte> memory)
    {
        return this.GetClientOrThrow(id).ProtectedSendAsync(memory);
    }

    /// <inheritdoc/>
    public Task SendAsync(string id, IRequestInfo requestInfo)
    {
        return this.GetClientOrThrow(id).ProtectedSendAsync(requestInfo);
    }

    private TcpSessionClient GetClientOrThrow(string id)
    {
        if (this.ProtectedTryGetClient(id, out var sessionClient))
        {
            return (TcpSessionClient)sessionClient;
        }
        ThrowHelper.ThrowClientNotFindException(id);
        return default;
    }

    #endregion Id发送

    #region Receiver

    /// <inheritdoc/>
    public void ClearReceiver()
    {
        this.ProtectedClearReceiver();
    }

    /// <inheritdoc/>
    public IReceiver<IReceiverResult> CreateReceiver()
    {
        return this.ProtectedCreateReceiver(this);
    }

    #endregion Receiver
}