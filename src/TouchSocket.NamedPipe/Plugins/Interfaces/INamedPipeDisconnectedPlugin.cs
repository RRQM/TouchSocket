using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 具有断开连接的插件接口
    /// </summary>
    public interface INamedPipeDisconnectedPlugin<in TClient> : IPlugin where TClient : INamedPipeClientBase
    {
        /// <summary>
        /// 会话断开后触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnNamedPipeDisconnected(TClient client, DisconnectEventArgs e);
    }

    /// <summary>
    /// INamedPipeDisconnectedPlugin
    /// </summary>
    public interface INamedPipeDisconnectedPlugin : INamedPipeDisconnectedPlugin<INamedPipeClientBase>
    {
    }
}