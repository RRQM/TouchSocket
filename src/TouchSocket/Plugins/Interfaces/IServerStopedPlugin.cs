using System;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// IServerStopedPlugin
    /// </summary>
    public interface IServerStopedPlugin<in TServer> : IPlugin where TServer : IService
    {
        /// <summary>
        /// 当服务器调用<see cref="IService.Stop"/>或者<see cref="IDisposable.Dispose"/>时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        Task OnServerStoped(TServer sender, ServiceStateEventArgs e);
    }

    /// <summary>
    /// IServerStopedPlugin
    /// </summary>
    public interface IServerStopedPlugin : IServerStopedPlugin<IService>
    {

    }
}