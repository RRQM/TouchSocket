using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 具有预备连接的插件接口
    /// </summary>
    public interface INamedPipeConnectingPlugin<in TClient> : IPlugin where TClient : INamedPipeClientBase
    {
        /// <summary>
        /// 在即将完成连接时触发。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnNamedPipeConnecting(TClient client, ConnectingEventArgs e);
    }

    /// <summary>
    /// INamedPipeConnectingPlugin
    /// </summary>
    public interface INamedPipeConnectingPlugin : INamedPipeConnectingPlugin<INamedPipeClientBase>
    {
    }
}