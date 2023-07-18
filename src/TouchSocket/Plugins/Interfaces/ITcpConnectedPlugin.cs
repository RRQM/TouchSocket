using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 具有完成连接动作的插件接口
    /// </summary>
    public interface ITcpConnectedPlugin<in TClient> : IPlugin where TClient: IClient
    {
        /// <summary>
        /// 客户端连接成功后触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnTcpConnected(TClient client, ConnectedEventArgs e);
    }

    /// <summary>
    /// ITcpConnectedPlugin
    /// </summary>
    public interface ITcpConnectedPlugin: ITcpConnectedPlugin<IClient>
    { 
    
    }
}
