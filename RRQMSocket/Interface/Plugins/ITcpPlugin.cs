using RRQMCore;

namespace RRQMSocket
{
    /// <summary>
    /// Tcp系插件接口
    /// </summary>
    public interface ITcpPlugin : IPlugin
    {
        /// <summary>
        /// 客户端连接成功后触发    
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnConnected(ITcpClientBase client, RRQMEventArgs e);

        /// <summary>
        ///在即将完成连接时触发。
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnConnecting(ITcpClientBase client, ClientOperationEventArgs e);

        /// <summary>
        /// 会话断开后触发
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnDisconnected(ITcpClientBase client, ClientDisconnectedEventArgs e);

        /// <summary>
        /// 在收到数据时触发
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnReceivedData(ITcpClientBase client, ReceivedDataEventArgs e);

        /// <summary>
        /// 当即将发送数据时，调用该方法在适配器之后，接下来即会发送数据。
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnSendingData(ITcpClientBase client, SendingEventArgs e);

        /// <summary>
        /// 当Client的ID被更改后触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void OnIDChanged(ITcpClientBase client, RRQMEventArgs e);
    }
}
