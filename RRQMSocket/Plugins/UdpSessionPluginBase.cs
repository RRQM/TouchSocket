using RRQMCore;
using RRQMCore.Log;

namespace RRQMSocket.Plugins
{
    /// <summary>
    /// Udp插件实现类
    /// </summary>
    public class UdpSessionPluginBase : DisposableObject, IUdpSessionPlugin
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ILog Logger { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPluginsManager PluginsManager { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void IUdpSessionPlugin.OnReceivedData(IUdpSession client, UdpReceivedDataEventArgs e)
        {
            this.OnReceivedData(client, e);
        }

        /// <summary>
        /// 收到数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnReceivedData(IUdpSession client, UdpReceivedDataEventArgs e)
        {

        }
    }
}
