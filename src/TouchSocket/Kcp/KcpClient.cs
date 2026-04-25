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
using System.Buffers;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// 基于KCP协议的客户端，以<see cref="UdpSessionBase"/>为底层UDP传输。
/// </summary>
/// <remarks>
/// 每个<see cref="KcpClient"/>维护一个<see cref="KcpCore"/>实例，
/// 通过后台定时器驱动KCP状态机，实现对远端<see cref="KcpService"/>的可靠连接。
/// </remarks>
public class KcpClient : UdpSessionBase
{
    private KcpCore? m_kcp;
    private CancellationTokenSource? m_cts;
    private EndPoint? m_remoteEndPoint;
    private readonly byte[] m_recvBuf = new byte[64 * 1024];

    /// <summary>
    /// 连接建立事件（KCP已就绪，可以发送数据）。
    /// </summary>
    public ConnectedEventHandler<KcpClient>? Connected;

    /// <summary>
    /// 连接断开事件。
    /// </summary>
    public ClosedEventHandler<KcpClient>? Closed;

    /// <summary>
    /// 收到应用层数据事件。
    /// </summary>
    public Func<KcpClient, KcpReceivedEventArgs, Task>? Received;

    #region 属性

    /// <summary>
    /// KCP 会话标识符。
    /// </summary>
    public uint Conv { get; private set; }

    /// <summary>
    /// 底层 <see cref="KcpCore"/> 实例，可用于调整KCP参数。
    /// </summary>
    public KcpCore? Kcp => this.m_kcp;

    /// <summary>
    /// 是否已连接（KCP已初始化且Update循环正在运行）。
    /// </summary>
    public bool IsConnected => this.m_kcp != null && this.m_cts?.IsCancellationRequested == false;

    #endregion

    #region 连接/断开

    /// <summary>
    /// 使用指定的会话标识符连接到服务端。
    /// </summary>
    /// <param name="conv">KCP会话标识符，需与服务端一致。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    public async Task ConnectAsync(uint conv, CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfNull(this.Config, nameof(this.Config));
        ThrowHelper.ThrowIfNull(this.Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty), "RemoteIPHost");

        this.Conv = conv;

        // 先停止已有连接
        await this.CloseKcpAsync().ConfigureDefaultAwait();

        this.m_remoteEndPoint = this.Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty)!.EndPoint;

        var kcp = new KcpCore(conv);
        kcp.Output = this.KcpOutput;
        this.m_kcp = kcp;

        this.m_cts = new CancellationTokenSource();

        // 启动 KCP Update 循环
        _ = EasyTask.SafeRun(() => this.UpdateLoopAsync(this.m_cts.Token));

        await this.OnKcpConnected(new ConnectedEventArgs()).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 断开KCP连接并释放资源。
    /// </summary>
    public async Task CloseKcpAsync()
    {
        var cts = Interlocked.Exchange(ref this.m_cts, null);
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
        }

        var kcp = Interlocked.Exchange(ref this.m_kcp, null);
        kcp?.Dispose();

        if (cts != null)
        {
            await this.OnKcpClosed(new ClosedEventArgs(string.Empty)).ConfigureDefaultAwait();
        }
    }

    #endregion

    #region 发送

    /// <summary>
    /// 将数据交给KCP发送队列（异步，由Update驱动实际出包）。
    /// </summary>
    /// <param name="memory">要发送的数据。</param>
    /// <param name="cancellationToken">取消令牌（当前未使用，为接口兼容保留）。</param>
    public Task SendAsync(ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
    {
        var kcp = this.m_kcp;
        ThrowHelper.ThrowIfNull(kcp, "KCP未连接，请先调用ConnectAsync");

        kcp!.Send(memory.Span);
        return EasyTask.CompletedTask;
    }

    #endregion

    #region 内部 - UDP回调与KCP逻辑

    /// <inheritdoc/>
    protected override async Task OnUdpReceived(UdpReceivedDataEventArgs e)
    {
        var kcp = this.m_kcp;
        if (kcp == null)
        {
            await base.OnUdpReceived(e).ConfigureDefaultAwait();
            return;
        }

        // 将原始UDP数据喂给KCP
        if (e.Memory.Length >= 4)
        {
            var result = kcp.Input(e.Memory.Span);
            if (result < 0)
            {
                // 忽略非本会话或格式错误的包
                return;
            }

            // 循环取出所有可读消息
            while (true)
            {
                var peekSize = kcp.PeekSize();
                if (peekSize <= 0)
                    break;

                using var owner = MemoryPool<byte>.Shared.Rent(peekSize);
                var buf = owner.Memory.Slice(0, peekSize);
                var n = kcp.Recv(buf.Span);
                if (n <= 0)
                    break;

                await this.OnKcpReceived(buf.Slice(0, n)).ConfigureDefaultAwait();
            }
        }
    }

    private void KcpOutput(ReadOnlySpan<byte> data)
    {
        var ep = this.m_remoteEndPoint;
        if (ep == null) return;

        // 租用数组保存 span 数据，以便异步发送
        var array = ArrayPool<byte>.Shared.Rent(data.Length);
        data.CopyTo(array);
        var mem = new ReadOnlyMemory<byte>(array, 0, data.Length);

        _ = EasyTask.SafeRun(async () =>
        {
            try
            {
                await this.ProtectedDefaultSendAsync(ep, mem, CancellationToken.None).ConfigureDefaultAwait();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        });
    }

    private async Task UpdateLoopAsync(CancellationToken token)
    {
        var kcp = this.m_kcp;
        if (kcp == null) return;

        while (!token.IsCancellationRequested)
        {
            var now = (uint)Environment.TickCount;
            kcp.Update(now);
            var nextMs = (int)(kcp.Check(now) - now);
            if (nextMs < 5) nextMs = 5;
            if (nextMs > 100) nextMs = 100;

            try
            {
                await Task.Delay(nextMs, token).ConfigureDefaultAwait();
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    #endregion

    #region 虚方法

    /// <summary>
    /// 当KCP连接建立时触发。重写此方法可自定义连接后逻辑（如设置KCP参数）。
    /// </summary>
    protected virtual async Task OnKcpConnected(ConnectedEventArgs e)
    {
        if (this.Connected != null)
        {
            await this.Connected.Invoke(this, e).ConfigureDefaultAwait();
        }
        await this.PluginManager.RaiseIKcpConnectedPluginAsync(this.Resolver, this, e).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 当KCP连接断开时触发。
    /// </summary>
    protected virtual async Task OnKcpClosed(ClosedEventArgs e)
    {
        if (this.Closed != null)
        {
            await this.Closed.Invoke(this, e).ConfigureDefaultAwait();
        }
        await this.PluginManager.RaiseIKcpClosedPluginAsync(this.Resolver, this, e).ConfigureDefaultAwait();
    }

    /// <summary>
    /// 当收到KCP应用层数据时触发。
    /// </summary>
    protected virtual async Task OnKcpReceived(ReadOnlyMemory<byte> data)
    {
        var e = new KcpReceivedEventArgs { Memory = data };
        if (this.Received != null)
        {
            await this.Received.Invoke(this, e).ConfigureDefaultAwait();
            if (e.Handled) return;
        }
        await this.PluginManager.RaiseIKcpClientReceivedPluginAsync(this.Resolver, this, e).ConfigureDefaultAwait();
    }

    #endregion

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            this.CloseKcpAsync().GetFalseAwaitResult();
        }
        base.SafetyDispose(disposing);
    }
}
