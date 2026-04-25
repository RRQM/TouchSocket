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
/// KCP服务端中表示单个客户端会话的对象。
/// </summary>
/// <remarks>
/// 每个<see cref="KcpSessionClient"/>封装一个独立的<see cref="KcpCore"/>实例，
/// 对应服务端收到的某个<c>(remoteEndPoint, conv)</c>组合。
/// 通过<see cref="SendAsync"/>向对端发送数据；收到数据时通过服务端的<see cref="KcpService.Received"/>触发回调。
/// </remarks>
public sealed class KcpSessionClient : IDisposable
{
    private readonly KcpService m_service;
    private readonly KcpCore m_kcp;
    private CancellationTokenSource? m_cts;

    internal KcpSessionClient(KcpService service, uint conv, EndPoint remoteEndPoint, IResolver resolver)
    {
        this.m_service = service;
        this.Conv = conv;
        this.RemoteEndPoint = remoteEndPoint;
        this.Resolver = resolver;

        this.m_kcp = new KcpCore(conv);
        this.m_kcp.Output = this.KcpOutput;
    }

    #region 属性

    /// <summary>
    /// KCP 会话标识符。
    /// </summary>
    public uint Conv { get; }

    /// <summary>
    /// 客户端远端地址。
    /// </summary>
    public EndPoint RemoteEndPoint { get; }

    /// <summary>
    /// 依赖注入解析器（来自所属服务）。
    /// </summary>
    public IResolver Resolver { get; }

    /// <summary>
    /// 底层 <see cref="KcpCore"/> 实例，可用于调整KCP参数。
    /// </summary>
    public KcpCore Kcp => this.m_kcp;

    /// <summary>
    /// 会话是否仍然活跃。
    /// </summary>
    public bool IsActive => this.m_cts?.IsCancellationRequested == false;

    #endregion

    #region 内部启动

    internal void Start()
    {
        this.m_cts = new CancellationTokenSource();
        _ = EasyTask.SafeRun(() => this.UpdateLoopAsync(this.m_cts.Token));
    }

    /// <summary>
    /// 将原始UDP数据喂入KCP并循环提取应用层消息。
    /// </summary>
    internal async Task InputAsync(ReadOnlyMemory<byte> data)
    {
        if (this.m_kcp.Input(data.Span) < 0)
            return;

        while (true)
        {
            var peekSize = this.m_kcp.PeekSize();
            if (peekSize <= 0) break;

            using var owner = MemoryPool<byte>.Shared.Rent(peekSize);
            var buf = owner.Memory.Slice(0, peekSize);
            var n = this.m_kcp.Recv(buf.Span);
            if (n <= 0) break;

            await this.m_service.OnKcpSessionReceived(this, buf.Slice(0, n)).ConfigureDefaultAwait();
        }
    }

    #endregion

    #region 发送

    /// <summary>
    /// 向该客户端发送应用层数据（KCP队列，由Update驱动实际出包）。
    /// </summary>
    /// <param name="memory">要发送的数据。</param>
    /// <param name="cancellationToken">取消令牌（当前未使用，为接口兼容保留）。</param>
    public Task SendAsync(ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
    {
        this.m_kcp.Send(memory.Span);
        return EasyTask.CompletedTask;
    }

    #endregion

    #region 私有

    private void KcpOutput(ReadOnlySpan<byte> data)
    {
        var array = ArrayPool<byte>.Shared.Rent(data.Length);
        data.CopyTo(array);
        var mem = new ReadOnlyMemory<byte>(array, 0, data.Length);
        var ep = this.RemoteEndPoint;

        _ = EasyTask.SafeRun(async () =>
        {
            try
            {
                await this.m_service.InternalSendAsync(ep, mem).ConfigureDefaultAwait();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        });
    }

    private async Task UpdateLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var now = (uint)Environment.TickCount;
            this.m_kcp.Update(now);
            var nextMs = (int)(this.m_kcp.Check(now) - now);
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

    /// <inheritdoc/>
    public void Dispose()
    {
        var cts = Interlocked.Exchange(ref this.m_cts, null);
        cts?.Cancel();
        cts?.Dispose();
        this.m_kcp.Dispose();
    }
}
