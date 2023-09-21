using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// IIdChangedPlugin
    /// </summary>
    public interface IIdChangedPlugin<in TClient> : IPlugin where TClient : IClient
    {
        /// <summary>
        /// 当Client的Id被更改后触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnIdChanged(TClient client, IdChangedEventArgs e);
    }

    /// <summary>
    /// IIdChangedPlugin
    /// </summary>
    public interface IIdChangedPlugin : IIdChangedPlugin<IClient>
    {
    }
}