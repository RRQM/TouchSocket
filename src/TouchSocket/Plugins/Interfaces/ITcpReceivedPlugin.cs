using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// ITcpReceivedPlugin
    /// </summary>
    public interface ITcpReceivedPlugin<in TClient> : IPlugin where TClient : ITcpClientBase
    {
        /// <summary>
        /// 在收到数据时触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnTcpReceived(TClient client, ReceivedDataEventArgs e);

    }

    /// <summary>
    /// ITcpReceivedPlugin
    /// </summary>
    public interface ITcpReceivedPlugin : ITcpReceivedPlugin<ITcpClientBase>
    {

    }
}
