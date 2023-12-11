using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// ITcpDmtpService
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public interface ITcpDmtpService<TClient>: ITcpDmtpServiceBase,ITcpService<TClient> where TClient : ITcpDmtpSocketClient
    {
    }

    /// <summary>
    /// ITcpDmtpService
    /// </summary>
    public interface ITcpDmtpService: ITcpDmtpService<TcpDmtpSocketClient>
    {

    }
}
