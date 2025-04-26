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
using System.Threading.Tasks.Sources;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.Http.WebSockets;

internal sealed partial class InternalWebSocket : BlockSegment<IWebSocketReceiveResult>, IWebSocket
{
    WebSocketReceiveBlockResult m_blockResult;

    public async Task Complete(string msg)
    {
        try
        {
            this.m_blockResult.IsCompleted = true;
            this.m_blockResult.Message = msg;
            await this.TriggerAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch
        {
        }
    }

    public ValueTask<IWebSocketReceiveResult> ReadAsync(CancellationToken token)
    {
        this.ThrowIfNotAllowAsyncRead();
        return base.ProtectedReadAsync(token);
    }
    internal async Task InputReceiveAsync(WSDataFrame dataFrame)
    {
        m_blockResult.DataFrame = dataFrame;
        await base.TriggerAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    protected override IWebSocketReceiveResult CreateResult(Action actionForDispose)
    {
        m_blockResult = new WebSocketReceiveBlockResult(actionForDispose);
        return m_blockResult;
    }
    private void ThrowIfNotAllowAsyncRead()
    {
        if (!this.m_allowAsyncRead)
        {
            ThrowHelper.ThrowNotSupportedException(TouchSocketHttpResource.NotAllowAsyncRead);
        }
    }
    #region Class
    sealed class WebSocketReceiveBlockResult : IWebSocketReceiveResult
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
            m_actionForDispose.Invoke();
        }
    }
    #endregion
}

