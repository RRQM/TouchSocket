using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace TouchSocket.Dmtp.AspNetCore
{
    public interface IWebsocketDmtpSocketClient : IWebsocketDmtpClientBase
    {
        /// <summary>
        /// Http上下文
        /// </summary>
        HttpContext HttpContext { get; }

        /// <summary>
        /// 包含该客户端的服务器。
        /// </summary>
        IWebsocketDmtpService Service { get; }
    }
}
