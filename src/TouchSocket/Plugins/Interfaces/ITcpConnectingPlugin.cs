using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 具有预备连接的插件接口
    /// </summary>
    public interface ITcpConnectingPlugin<in TClient> : IPlugin where TClient: IClient
    {
        /// <summary>
        /// 在即将完成连接时触发。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnTcpConnecting(TClient client, ConnectingEventArgs e);
    }

    /// <summary>
    /// ITcpConnectingPlugin
    /// </summary>
    public interface ITcpConnectingPlugin: ITcpConnectingPlugin<IClient>
    { 
    
    }
}
