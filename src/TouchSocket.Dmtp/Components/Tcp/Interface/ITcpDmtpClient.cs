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
        /// <summary>
        /// 建立Tcp连接，并且执行握手。
        /// </summary>
        /// <param name="token">可取消令箭</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        ITcpDmtpClient Connect(CancellationToken token, int timeout = 5000);

        /// <summary>
        /// 建立Tcp连接，并且执行握手。
        /// </summary>
        /// <param name="token">可取消令箭</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        Task<ITcpDmtpClient> ConnectAsync(CancellationToken token, int timeout = 5000);
    }
}