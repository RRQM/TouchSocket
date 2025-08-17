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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Http.WebSockets;

internal sealed partial class InternalWebSocket : SafetyDisposableObject, IWebSocket
{
    private readonly AsyncExchange<WSDataFrame> m_asyncExchange = new();

    private string msg;

    public void Complete(string msg)
    {
        try
        {
            this.m_asyncExchange.Complete();
            this.msg = msg;
        }
        catch
        {
        }
    }

    public async ValueTask<IWebSocketReceiveResult> ReadAsync(CancellationToken token)
    {
        this.ThrowIfNotAllowAsyncRead();
        var readLease = await this.m_asyncExchange.ReadAsync(token);
        var frame = readLease.Value;
        return new WebSocketReceiveBlockResult(readLease.Dispose)
        {
            IsCompleted = readLease.IsCompleted,
            DataFrame = frame,
            Message = this.msg
        };
    }

    internal ValueTask InputReceiveAsync(WSDataFrame dataFrame, CancellationToken token)
    {
        return this.m_asyncExchange.WriteAsync(dataFrame, token);
    }

    private void ThrowIfNotAllowAsyncRead()
    {
        if (!this.m_allowAsyncRead)
        {
            ThrowHelper.ThrowNotSupportedException(TouchSocketHttpResource.NotAllowAsyncRead);
        }
    }

    #region Class

    private sealed class WebSocketReceiveBlockResult : IWebSocketReceiveResult
    {
        private readonly Action m_actionForDispose;

        public WebSocketReceiveBlockResult(Action actionForDispose)
        {
            this.m_actionForDispose = actionForDispose;
        }

        public WSDataFrame DataFrame { get; set; }

        /// <summary>
        /// 获取表示内存处理是否完成的布尔值。
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// 获取处理结果的消息。
        /// </summary>
        public string Message { get; set; }

        void IDisposable.Dispose()
        {
            this.m_actionForDispose.Invoke();
        }
    }

    #endregion Class
}