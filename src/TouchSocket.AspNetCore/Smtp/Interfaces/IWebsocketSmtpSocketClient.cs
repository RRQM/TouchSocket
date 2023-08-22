using Microsoft.AspNetCore.Http;

namespace TouchSocket.Dmtp.AspNetCore
{
    /// <summary>
    /// 基于WebSocket协议的Dmtp服务器辅助客户端。
    /// </summary>
    public interface IWebSocketDmtpSocketClient : IWebSocketDmtpClientBase
    {
        /// <summary>
        /// Http上下文
        /// </summary>
        HttpContext HttpContext { get; }

        /// <summary>
        /// 包含该客户端的服务器。
        /// </summary>
        IWebSocketDmtpService Service { get; }
    }
}
