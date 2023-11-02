using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 基于Dmtp协议的Tcp客户端接口
    /// </summary>
    public interface ITcpDmtpClient : ITcpDmtpClientBase, ITcpClient
    {
    }
}