using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 即将断开连接(仅主动断开时有效)。
    /// </summary>
    public interface ITcpDisconnectingPlugin<in TClient> : IPlugin where TClient : ITcpClientBase
    {

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// <para>
        /// 当主动调用Close断开时。
        /// </para>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnTcpDisconnecting(TClient client, DisconnectEventArgs e);
    }

    /// <summary>
    /// ITcpDisconnectingPlugin
    /// </summary>
    public interface ITcpDisconnectingPlugin : ITcpDisconnectingPlugin<ITcpClientBase>
    {

    }
}
