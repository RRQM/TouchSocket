using TouchSocket.Core;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 命名管道在即将连接时事件
    /// </summary>
    public class NamedPipeConnectingEventArgs : MsgPermitEventArgs
    {
        /// <summary>
        /// 客户端Id。该Id的赋值，仅在服务器适用。
        /// </summary>
        public string Id { get; set; }
    }
}