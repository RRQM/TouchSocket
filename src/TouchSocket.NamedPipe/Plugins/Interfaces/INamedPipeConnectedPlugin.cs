using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 具有完成连接动作的插件接口
    /// </summary>
    public interface INamedPipeConnectedPlugin<in TClient> : IPlugin where TClient : INamedPipeClientBase
    {
        /// <summary>
        /// 客户端连接成功后触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnNamedPipeConnected(TClient client, ConnectedEventArgs e);
    }

    /// <summary>
    /// INamedPipeConnectedPlugin
    /// </summary>
    public interface INamedPipeConnectedPlugin : INamedPipeConnectedPlugin<INamedPipeClientBase>
    {
    }
}