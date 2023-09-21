using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// INamedPipeReceivedPlugin
    /// </summary>
    public interface INamedPipeReceivedPlugin<in TClient> : IPlugin where TClient : INamedPipeClientBase
    {
        /// <summary>
        /// 在收到数据时触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnNamedPipeReceived(TClient client, ReceivedDataEventArgs e);
    }

    /// <summary>
    /// INamedPipeReceivedPlugin
    /// </summary>
    public interface INamedPipeReceivedPlugin : INamedPipeReceivedPlugin<INamedPipeClientBase>
    {
    }
}