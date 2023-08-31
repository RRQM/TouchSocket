using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// UdpDmtp终端接口
    /// </summary>
    public interface IUdpDmtpClient : IDmtpActorObject
    {
        /// <summary>
        /// 默认远程终结点。
        /// </summary>
        EndPoint EndPoint { get; }

        /// <summary>
        /// 默认通信的udp终端。
        /// </summary>
        IUdpSession UdpSession { get; }
    }
}
