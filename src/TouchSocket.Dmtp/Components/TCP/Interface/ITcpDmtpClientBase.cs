using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 基于Dmtp协议的Tcp终端接口
    /// </summary>
    public interface ITcpDmtpClientBase : ITcpClientBase, IDmtpActorObject
    {
    }
}