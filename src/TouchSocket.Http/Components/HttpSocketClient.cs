//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// http辅助类
    /// </summary>
    public class HttpSocketClient : SocketClient, IHttpSocketClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public HttpSocketClient()
        {
            this.Protocol = Protocol.Http;
        }

        /// <inheritdoc/>
        protected override async Task OnConnecting(ConnectingEventArgs e)
        {
            this.SetDataHandlingAdapter(new HttpServerDataHandlingAdapter());
            await base.OnConnecting(e);
        }

        /// <inheritdoc/>
        protected override async Task ReceivedData(ReceivedDataEventArgs e)
        {
            if (e.RequestInfo is HttpRequest request)
            {
                await this.OnReceivedHttpRequest(request);
            }
            await base.ReceivedData(e);
        }

        /// <summary>
        /// 当收到到Http请求时。覆盖父类方法将不会触发插件。
        /// </summary>
        protected virtual Task OnReceivedHttpRequest(HttpRequest request)
        {
            if (this.PluginsManager.GetPluginCount(nameof(IHttpPlugin.OnHttpRequest)) > 0)
            {
                var e = new HttpContextEventArgs(new HttpContext(request));

                return this.PluginsManager.RaiseAsync(nameof(IHttpPlugin.OnHttpRequest), this, e);
            }

            return EasyTask.CompletedTask;
        }
    }
}