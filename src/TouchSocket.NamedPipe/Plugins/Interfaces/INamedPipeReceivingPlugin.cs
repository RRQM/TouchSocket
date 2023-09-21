using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// INamedPipeReceivingPlugin
    /// </summary>
    public interface INamedPipeReceivingPlugin<in TClient> : IPlugin where TClient : INamedPipeClientBase
    {
        /// <summary>
        /// 在刚收到数据时触发，即在适配器之前。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnNamedPipeReceiving(TClient client, ByteBlockEventArgs e);
    }

    /// <summary>
    /// INamedPipeReceivingPlugin
    /// </summary>
    public interface INamedPipeReceivingPlugin : INamedPipeReceivingPlugin<INamedPipeClientBase>
    {
    }
}