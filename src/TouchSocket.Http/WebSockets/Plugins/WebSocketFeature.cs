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

using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// 基于Http的WebSocket的扩展。
    /// <para>此组件只能挂载在<see cref="HttpService"/>中</para>
    /// </summary>
    [PluginOption(Singleton = true)]
    public sealed class WebSocketFeature : PluginBase
    {
        /// <summary>
        /// 自动响应Close报文
        /// </summary>
        public static readonly DependencyProperty<bool> AutoCloseProperty =
           DependencyProperty<bool>.Register("AutoClose", true);

        /// <summary>
        /// 自动响应Ping报文
        /// </summary>
        public static readonly DependencyProperty<bool> AutoPongProperty =
           DependencyProperty<bool>.Register("AutoPong", false);

        private string m_wSUrl = "/ws";

        /// <summary>
        /// WebSocketFeature
        /// </summary>
        public WebSocketFeature()
        {
            this.VerifyConnection = this.ThisVerifyConnection;
        }

        /// <summary>
        /// 是否默认处理Close报文。
        /// </summary>
        public bool AutoClose { get; set; } = true;

        /// <summary>
        /// 当收到ping报文时，是否自动回应pong。
        /// </summary>
        public bool AutoPong { get; set; }

        /// <summary>
        /// 验证连接
        /// </summary>
        public Func<IHttpSessionClient, HttpContext, Task<bool>> VerifyConnection { get; set; }

        /// <summary>
        /// 用于WebSocket连接的路径，默认为“/ws”
        /// <para>如果设置为null或空，则意味着所有的连接都将解释为WS</para>
        /// </summary>
        public string WSUrl
        {
            get => this.m_wSUrl;
            set => this.m_wSUrl = string.IsNullOrEmpty(value) ? "/" : value;
        }

        /// <summary>
        /// 不处理Close报文。
        /// </summary>
        /// <returns></returns>
        public WebSocketFeature NoAutoClose()
        {
            this.AutoClose = false;
            return this;
        }

        /// <summary>
        /// 验证连接
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public WebSocketFeature SetVerifyConnection(Func<IHttpSessionClient, HttpContext, bool> func)
        {
            this.VerifyConnection = async (client, context) =>
            {
                await EasyTask.CompletedTask.ConfigureFalseAwait();
                return func.Invoke(client, context);
            };
            return this;
        }

        /// <summary>
        /// 验证连接
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public WebSocketFeature SetVerifyConnection(Func<IHttpSessionClient, HttpContext, Task<bool>> func)
        {
            this.VerifyConnection = func;
            return this;
        }

        /// <summary>
        /// 用于WebSocket连接的路径，默认为“/ws”
        /// <para>如果设置为null或空，则意味着所有的连接都将解释为WS</para>
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public WebSocketFeature SetWSUrl(string url)
        {
            this.WSUrl = url;
            return this;
        }

        /// <summary>
        /// 当收到ping报文时，自动回应pong。
        /// </summary>
        /// <returns></returns>
        public WebSocketFeature UseAutoPong()
        {
            this.AutoPong = true;
            return this;
        }

        /// <inheritdoc/>
        protected override void Loaded(IPluginManager pluginManager)
        {
            base.Loaded(pluginManager);
            pluginManager.Add<IHttpSessionClient, HttpContextEventArgs>(typeof(IHttpPlugin), this.OnHttpRequest);
        }

        private async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
        {
            if (client.Protocol == Protocol.Http)
            {
                if (await this.VerifyConnection.Invoke(client, e.Context).ConfigureFalseAwait())
                {
                    e.Handled = true;
                    await client.SwitchProtocolToWebSocketAsync(e.Context).ConfigureFalseAwait();
                    if (!this.AutoClose)
                    {
                        client.SetValue(AutoCloseProperty, false);
                    }
                    if (this.AutoPong)
                    {
                        client.SetValue(AutoPongProperty, true);
                    }
                    return;
                }
            }
            await e.InvokeNext().ConfigureFalseAwait();
        }

        private async Task<bool> ThisVerifyConnection(IHttpSessionClient client, HttpContext context)
        {
            await EasyTask.CompletedTask.ConfigureFalseAwait();
            if (context.Request.Method == HttpMethod.Get)
            {
                if (this.WSUrl == "/" || context.Request.UrlEquals(this.WSUrl))
                {
                    return true;
                }
            }

            return false;
        }
    }
}