using RRQMCore;
using RRQMCore.Log;

namespace RRQMSocket.Plugins
{
    /// <summary>
    /// 插件实现基类
    /// </summary>
    public class TcpPluginBase : DisposableObject, ITcpPlugin
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
        void ITcpPlugin.OnConnecting(ITcpClientBase client, ClientOperationEventArgs e)
        {
            this.OnConnecting(client, e);
        }

        void ITcpPlugin.OnDisconnected(ITcpClientBase client, ClientDisconnectedEventArgs e)
        {
            this.OnDisconnected(client, e);
        }

        void ITcpPlugin.OnIDChanged(ITcpClientBase client, RRQMEventArgs e)
        {
            this.OnIDChanged(client, e);
        }

        void ITcpPlugin.OnReceivedData(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            this.OnReceivedData(client, e);
        }

        void ITcpPlugin.OnSendingData(ITcpClientBase client, SendingEventArgs e)
        {
            this.OnSending(client, e);
        }

        void ITcpPlugin.OnConnected(ITcpClientBase client, RRQMEventArgs e)
        {
            this.OnConnected(client, e);
        }

        /// <summary>
        /// 成功建立连接
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnConnected(ITcpClientBase client, RRQMEventArgs e)
        {

        }

        /// <summary>
        /// 在请求连接时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnConnecting(ITcpClientBase client, ClientOperationEventArgs e)
        {

        }

        /// <summary>
        /// 在断开连接时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(ITcpClientBase client, ClientDisconnectedEventArgs e)
        {

        }

        /// <summary>
        /// 当Client的ID被更改后触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnIDChanged(ITcpClientBase client, RRQMEventArgs e)
        {

        }

        /// <summary>
        /// 在收到数据时触发
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数，当设置e.Handled=true时，终止向下传递</param>
        protected virtual void OnReceivedData(ITcpClientBase client, ReceivedDataEventArgs e)
        {

        }

        /// <summary>
        /// 当即将发送数据时，调用该方法在适配器之后，接下来即会发送数据。
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数，当设置e.IsPermitOperation=false时，中断发送。</param>
        protected virtual void OnSending(ITcpClientBase client, SendingEventArgs e)
        {

        }
    }
}
