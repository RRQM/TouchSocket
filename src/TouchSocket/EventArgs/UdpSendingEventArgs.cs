using System;
using System.Net;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 提供与UDP发送操作相关的事件处理方法。
    /// </summary>
    /// <remarks>
    /// 该类继承自<see cref="SendingEventArgs"/>，专门化处理UDP发送事件，
    /// 包含了发送数据的目的端点信息。
    /// </remarks>
    public class UdpSendingEventArgs : SendingEventArgs
    {
        /// <summary>
        /// 初始化<see cref="UdpSendingEventArgs"/>类的新实例。
        /// </summary>
        /// <param name="memory">要发送的数据内存区块。</param>
        /// <param name="endPoint">数据发送的目的端点。</param>
        public UdpSendingEventArgs(in ReadOnlyMemory<byte> memory, EndPoint endPoint) : base(memory)
        {
            this.EndPoint = endPoint;
        }

        /// <summary>
        /// 获取发送数据的目的端点。
        /// </summary>
        public EndPoint EndPoint { get; }
    }
}
