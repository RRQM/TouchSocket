using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// IServerStartedPlugin
    /// </summary>
    public interface IServerStartedPlugin<in TServer> : IPlugin where TServer : IService
    {

        /// <summary>
        /// 当服务器执行<see cref="IService.Start"/>后时。
        /// <para>
        /// 注意：此处并不表示服务器成功启动，具体状态请看<see cref="ServiceStateEventArgs.ServerState"/>
        /// </para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        Task OnServerStarted(TServer sender, ServiceStateEventArgs e);

    }

    /// <summary>
    /// IServerStartedPlugin
    /// </summary>
    public interface IServerStartedPlugin : IServerStartedPlugin<IService>
    {

    }
}
