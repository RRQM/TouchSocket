using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace TouchSocket.Smtp.AspNetCore
{
    public interface IWebsocketSmtpSocketClient : IWebsocketSmtpClientBase
    {
        /// <summary>
        /// Http上下文
        /// </summary>
        HttpContext HttpContext { get; }

        /// <summary>
        /// 包含该客户端的服务器。
        /// </summary>
        IWebsocketSmtpService Service { get; }
    }
}
