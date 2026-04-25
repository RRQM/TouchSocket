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

#nullable enable

using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// 基于KCP协议的服务端，以<see cref="UdpSessionBase"/>为底层UDP传输。
/// </summary>
/// <remarks>
/// <see cref="KcpService"/>监听指定UDP端口，通过收到数据包头部的<c>conv</c>字段
/// 区分不同的客户端会话，每个会话由一个独立的<see cref="KcpSessionClient"/>负责。
/// 新会话首次收到任何数据包时自动创建。
/// </remarks>
public class KcpService : UdpSessionBase
{
    private readonly ConcurrentDictionary<EndPoint, KcpSessionClient> m_sessions
        = new ConcurrentDictionary<EndPoint, KcpSessionClient>();

    /// <summary>
    /// 新KCP会话连接时触发。
    /// </summary>
    public ConnectedEventHandler<KcpSessionClient>? Connected;

    /// <summary>
    /// KCP会话断开时触发。
    /// </summary>
    public ClosedEventHandler<KcpSessionClient>? Closed;

    /// <summary>
    /// 收到来自某个KCP会话的应用层数据时触发。
    /// </summary>
    public Func<KcpService, KcpSessionClient, KcpReceivedEventArgs, Task>? Received;

    #region 属性

    /// <summary>
    /// 获取当前所有活跃的KCP会话（以远端EndPoint为键）。
    /// </summary>
    public IReadOnlyDictionary<EndPoint, KcpSessionClient> Sessions => this.m_sessions;

    #endregion

    #region 会话管理

    /// <summary>
    /// 主动关闭并移除指定EndPoint对应的KCP会话。
    /// </summary>
    public async Task CloseSessionAsync(EndPoint remoteEndPoint, string msg = "")
    {
        if (this.m_sessions.TryRemove(remoteEndPoint, out var session))
        {
            session.Dispose();
            await this.OnKcpClosed(session, new ClosedEventArgs(msg)).ConfigureDefaultAwait();
        }
    }

    #endregion

    #region 内部 - UDP回调与KCP逻辑

    /// <summary>
    /// 供<see cref="KcpSessionClient"/>调用的内部UDP发送方法。
    /// </summary>
    internal Task InternalSendAsync(EndPoint remoteEndPoint, ReadOnlyMemory<byte> memory)
    {
        return this.ProtectedDefaultSendAsync(remoteEndPoint, memory, CancellationToken.None);
    }

    /// <inheritdoc/>
    protected override async Task OnUdpReceived(UdpReceivedDataEventArgs e)
    {
        // KCP 包头至少需要 4 字节的 conv 字段
        if (e.Memory.Length < 4)
        {
            await base.OnUdpReceived(e).ConfigureDefaultAwait();
            return;
        }

        var ep = e.EndPoint!;
        var conv = BinaryPrimitives.ReadUInt32LittleEndian(e.Memory.Span);

        var session = this.m_sessions.GetOrAdd(ep, _ => this.CreateSession(conv, ep));

        await session.InputAsync(e.Memory).ConfigureDefaultAwait();
    }

    private KcpSessionClient CreateSession(uint conv, EndPoint ep)
    {
        var session = new KcpSessionClient(this, conv, ep, this.Resolver);
        session.Start();

        // 触发连接事件（fire-and-forget，避免在 GetOrAdd 回调中 await）
        _ = EasyTask.SafeRun(async () =>
        {
            await this.OnKcpConnected(session, new ConnectedEventArgs()).ConfigureDefaultAwait();
        });

        return session;
    }

    /// <summary>
    /// 当某个KCP会话收到应用层数据时由<see cref="KcpSessionClient"/>回调（内部使用）。
    /// </summary>
    internal async Task OnKcpSessionReceived(KcpSessionClient session, ReadOnlyMemory<byte> data)
    {
        await this.OnKcpReceived(session, data).ConfigureDefaultAwait();
    }

    #endregion

    #region 虚方法

    /// <summary>
    /// 当新的KCP会话连接时触发。重写此方法可在连接建立时调整KCP参数。
    /// </summary>
    protected virtual async Task OnKcpConnected(KcpSessionClient session, ConnectedEventArgs e)
    {
        if (this.Connected != null)
        {
            await this.Connected.Invoke(session, e).ConfigureDefaultAwait();
        }
        await this.PluginManager.RaiseIKcpConnectedPluginAsync(this.Resolver, session, e).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 当KCP会话断开时触发。
    /// </summary>
    protected virtual async Task OnKcpClosed(KcpSessionClient session, ClosedEventArgs e)
    {
        if (this.Closed != null)
        {
            await this.Closed.Invoke(session, e).ConfigureDefaultAwait();
        }
        await this.PluginManager.RaiseIKcpClosedPluginAsync(this.Resolver, session, e).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 当收到某个KCP会话的应用层数据时触发。
    /// </summary>
    protected virtual async Task OnKcpReceived(KcpSessionClient session, ReadOnlyMemory<byte> data)
    {
        var e = new KcpReceivedEventArgs { Memory = data };
        if (this.Received != null)
        {
            await this.Received.Invoke(this, session, e).ConfigureDefaultAwait();
            if (e.Handled) return;
        }
        await this.PluginManager.RaiseIKcpSessionReceivedPluginAsync(this.Resolver, session, e).ConfigureDefaultAwait();
    }

    #endregion

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var session in this.m_sessions.Values)
            {
                session.Dispose();
            }
            this.m_sessions.Clear();
        }
        base.SafetyDispose(disposing);
    }
}
