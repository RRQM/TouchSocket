//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace TouchSocket.Dmtp.AspNetCore
{
    /// <summary>
    /// WebSocketDmtpMiddleware中间件
    /// </summary>
    public class WebSocketDmtpMiddleware
    {
        private readonly RequestDelegate m_next;
        private readonly IWebSocketDmtpService m_websocketDmtpService;
        private string m_url = "/websocketdmtp";

        /// <summary>
        /// 实例化一个中间件
        /// </summary>
        /// <param name="m_url"></param>
        /// <param name="next"></param>
        /// <param name="rpcService"></param>
        public WebSocketDmtpMiddleware(string m_url, RequestDelegate next, IWebSocketDmtpService rpcService)
        {
            this.Url = m_url;
            this.m_next = next ?? throw new ArgumentNullException(nameof(next));
            this.m_websocketDmtpService = rpcService;
        }

        /// <summary>
        /// Url
        /// </summary>
        public string Url
        {
            get => this.m_url;
            set => this.m_url = string.IsNullOrEmpty(value) ? "/websocketdmtp" : value;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Equals(this.Url, StringComparison.OrdinalIgnoreCase))
            {
                await this.m_websocketDmtpService.SwitchClientAsync(context).ConfigureAwait(false);
            }
            else
            {
                await this.m_next(context).ConfigureAwait(false);
            }
        }
    }
}