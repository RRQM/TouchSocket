using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 具有断开连接的插件接口
    /// </summary>
    public interface ITcpDisconnectedPlguin<in TClient> : IPlugin where TClient: IClient
    {
        /// <summary>
        /// 会话断开后触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnTcpDisconnected(TClient client, DisconnectEventArgs e);
    }

    /// <summary>
    /// ITcpDisconnectedPlguin
    /// </summary>
    public interface ITcpDisconnectedPlguin: ITcpDisconnectedPlguin<IClient>
    { 
    
    }
}
