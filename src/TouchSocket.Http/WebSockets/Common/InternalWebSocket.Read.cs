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

using System.Net.WebSockets;
using TouchSocket.Resources;

namespace TouchSocket.Http.WebSockets;

internal sealed partial class InternalWebSocket : SafetyDisposableObject, IWebSocket
{
    private readonly AsyncExchange<WSDataFrame> m_asyncExchange = new();
    private string m_completeMsg;

    public void Complete(string msg)
    {
        try
        {
            this.m_completeMsg = msg;
            this.m_asyncExchange.Complete();
        }
        catch
        {
        }
    }

    public async ValueTask<WebSocketReceiveResult> ReadAsync(CancellationToken cancellationToken)
    {
        this.ThrowIfNotAllowAsyncRead();
        var readLease = await this.m_asyncExchange.ReadAsync(cancellationToken)
            .ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        var frame = readLease.Value;
        var message = readLease.IsCompleted ? this.m_completeMsg : null;
        return new WebSocketReceiveResult(readLease,frame,message);
    }

    internal ValueTask<bool> InputReceiveAsync(WSDataFrame dataFrame, CancellationToken cancellationToken)
    {
        return this.m_asyncExchange.WriteAsync(dataFrame, cancellationToken);
    }

    private void ThrowIfNotAllowAsyncRead()
    {
        if (!this.m_allowAsyncRead)
        {
            ThrowHelper.ThrowNotSupportedException(TouchSocketHttpResource.NotAllowAsyncRead);
        }
    }
}