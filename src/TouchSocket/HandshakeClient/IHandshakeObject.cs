using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 具有握手连接的对象。
    /// </summary>
    public interface IHandshakeObject
    {
        /// <summary>
        /// 只是当前客户端是否已经完成握手连接。
        /// </summary>
        bool IsHandshaked { get; }
    }
}
