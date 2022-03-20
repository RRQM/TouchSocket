using RRQMSocket.Http;

namespace RRQMSocket.WebSocket
{
    /// <summary>
    /// WebSocket插件
    /// </summary>
    public interface IWebSocketPlugin : IPlugin
    {
        /// <summary>
        /// 表示在即将握手连接时。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void OnHandshaking(ITcpClientBase client, HttpContextEventArgs e);

        /// <summary>
        /// 表示完成握手后。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void OnHandshaked(ITcpClientBase client, HttpContextEventArgs e);

        /// <summary>
        /// 当收到WS数据时。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void OnHandleWSDataFrame(ITcpClientBase client, WSDataFrameEventArgs e);
    }
}
