using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 握手状态。
    /// </summary>
    public enum HandshakeStatus:byte
    {
        /// <summary>
        /// 标识没有任何操作
        /// </summary>
        None,

        /// <summary>
        /// 标识正在握手
        /// </summary>
        Handshaking,

        /// <summary>
        /// 标识已经完成握手
        /// </summary>
        Handshaked,

        /// <summary>
        /// 标识正在执行关闭
        /// </summary>
        Closing,

        /// <summary>
        /// 标识已经关闭
        /// </summary>
        Closed
    }
}
