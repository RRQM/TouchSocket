//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using TouchSocket.Core.ByteManager;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// http辅助类
    /// </summary>
    public class HttpSocketClient : SocketClient, IHttpClientBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public HttpSocketClient()
        {
            this.Protocol = Protocol.Http;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnConnecting(ClientOperationEventArgs e)
        {
            this.SetDataHandlingAdapter(new HttpServerDataHandlingAdapter());
            base.OnConnecting(e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (requestInfo is HttpRequest request)
            {
                this.OnReceivedHttpRequest(request);
            }
        }

        /// <summary>
        /// 当收到到Http请求时。覆盖父类方法将不会触发插件。
        /// </summary>
        protected virtual void OnReceivedHttpRequest(HttpRequest request)
        {
            HttpContextEventArgs args = new HttpContextEventArgs(new HttpContext(request));

            switch (request.Method)
            {
                case TouchSocketHttpUtility.Get:
                    {
                        this.PluginsManager.Raise<IHttpPlugin>("OnGet", this, args);
                        break;
                    }
                case TouchSocketHttpUtility.Post:
                    {
                        this.PluginsManager.Raise<IHttpPlugin>("OnPost", this, args);
                        break;
                    }
                case TouchSocketHttpUtility.Put:
                    {
                        this.PluginsManager.Raise<IHttpPlugin>("OnPut", this, args);
                        break;
                    }
                case TouchSocketHttpUtility.Delete:
                    {
                        this.PluginsManager.Raise<IHttpPlugin>("OnDelete", this, args);
                        break;
                    }
                default:
                    this.PluginsManager.Raise<IHttpPlugin>("OnReceivedOtherHttpRequest", this, args);
                    break;
            }
        }
    }
}