namespace RRQMSocket
{
    /// <summary>
    /// Udp会话插件
    /// </summary>
    public interface IUdpSessionPlugin : IPlugin
    {
        /// <summary>
        /// 在收到数据时触发
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnReceivedData(IUdpSession client, UdpReceivedDataEventArgs e);
    }
}
