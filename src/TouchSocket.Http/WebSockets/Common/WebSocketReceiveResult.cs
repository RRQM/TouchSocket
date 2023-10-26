using System;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// WebSocketReceiveResult
    /// </summary>
    public struct WebSocketReceiveResult : IDisposable
    {
        private Action m_disAction;

        /// <summary>
        /// WebSocketReceiveResult
        /// </summary>
        /// <param name="disAction"></param>
        /// <param name="dataFrame"></param>
        public WebSocketReceiveResult(Action disAction, WSDataFrame dataFrame)
        {
            this.m_disAction = disAction;
            this.DataFrame = dataFrame;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.m_disAction?.Invoke();
        }

        /// <summary>
        /// WebSocket数据帧
        /// </summary>
        public WSDataFrame DataFrame { get; private set; }

        /// <summary>
        /// 连接已关闭
        /// </summary>
        public bool IsClosed => this.DataFrame == null;
    }
}