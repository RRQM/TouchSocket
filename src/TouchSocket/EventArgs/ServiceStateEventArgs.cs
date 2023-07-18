using System;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 服务器状态事件参数
    /// </summary>
    public class ServiceStateEventArgs : MsgPermitEventArgs
    {
        /// <summary>
        /// 服务器状态事件参数
        /// </summary>
        /// <param name="serverState"></param>
        /// <param name="exception"></param>
        public ServiceStateEventArgs(ServerState serverState, Exception exception)
        {
            this.ServerState = serverState;
            this.Exception = exception;
        }

        /// <summary>
        /// 服务器状态
        /// </summary>
        public ServerState ServerState { get; }

        /// <summary>
        /// 异常类
        /// </summary>
        public Exception Exception { get; }
    }
}
