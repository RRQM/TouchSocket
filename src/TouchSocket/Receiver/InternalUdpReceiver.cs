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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets;

internal sealed class InternalUdpReceiver : SafetyDisposableObject, IReceiver<IUdpReceiverResult>
{
    #region 字段

    private readonly IReceiverClient<IUdpReceiverResult> m_client;
    private readonly AsyncExchange<(ReadOnlyMemory<byte>, IRequestInfo, EndPoint)> m_asyncExchange = new();
    private string m_msg;

    #endregion 字段

   
    public InternalUdpReceiver(IReceiverClient<IUdpReceiverResult> client)
    {
        this.m_client = client;
    }

    public void Complete(string msg)
    {
        this.m_msg = msg;
        this.m_asyncExchange.Complete();
    }

    /// <inheritdoc/>
    public async Task InputReceive(System.Net.EndPoint remoteEndPoint, ReadOnlyMemory<byte> memory, IRequestInfo requestInfo, CancellationToken token)
    {
        if (this.DisposedValue)
        {
            return;
        }
        await this.m_asyncExchange.WriteAsync((memory, requestInfo, remoteEndPoint), token);
    }

    /// <inheritdoc/>
    public async ValueTask<IUdpReceiverResult> ReadAsync(CancellationToken token)
    {
        var readLease = await this.m_asyncExchange.ReadAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        var (memory, requestInfo, endpoint) = readLease.Value;
        return new UdpReceiverResult(readLease.Dispose)
        {
            IsCompleted = readLease.IsCompleted,
            Memory = memory,
            RequestInfo = requestInfo,
            Message = this.m_msg,
            EndPoint = endpoint
        };
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            this.m_client.ClearReceiver();
        }
    }
}