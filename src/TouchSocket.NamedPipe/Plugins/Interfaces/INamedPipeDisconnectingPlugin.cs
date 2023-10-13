using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 即将断开连接(仅主动断开时有效)。
    /// </summary>
    public interface INamedPipeDisconnectingPlugin<in TClient> : IPlugin where TClient : INamedPipeClientBase
    {
        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnNamedPipeDisconnecting(TClient client, DisconnectEventArgs e);
    }

    /// <summary>
    /// INamedPipeDisconnectingPlugin
    /// </summary>
    public interface INamedPipeDisconnectingPlugin : INamedPipeDisconnectingPlugin<INamedPipeClientBase>
    {
    }
}