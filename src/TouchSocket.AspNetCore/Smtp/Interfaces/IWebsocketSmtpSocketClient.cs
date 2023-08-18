using Microsoft.AspNetCore.Http;

namespace TouchSocket.Dmtp.AspNetCore
{
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
