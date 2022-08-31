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
using System;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Plugins;
using TouchSocket.Http.Plugins;
using TouchSocket.Sockets;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// 基于Http的WebSocket的扩展。
    /// <para>此组件只能挂载在<see cref="HttpService"/>中</para>
    /// </summary>
    [SingletonPlugin]
    public class WebSocketServerPlugin : HttpPluginBase
    {
        /// <summary>
        /// 表示是否完成WS握手
        /// </summary>
        public static readonly DependencyProperty HandshakedProperty =
            DependencyProperty.Register("Handshaked", typeof(bool), typeof(WebSocketServerPlugin), false);

        /// <summary>
        /// 表示WebSocketVersion
        /// </summary>
        public static readonly DependencyProperty WebSocketVersionProperty =
            DependencyProperty.Register("WebSocketVersion", typeof(string), typeof(WebSocketServerPlugin), "13");

        private string wSUrl = "/ws";

        /// <summary>
        /// 处理WS数据的回调
        /// </summary>
        public Action<ITcpClientBase, WSDataFrameEventArgs> HandleWSDataFrameCallback { get; set; }

        /// <summary>
        /// 用于WebSocket连接的路径，默认为“/ws”
        /// <para>如果设置为null或空，则意味着所有的连接都将解释为WS</para>
        /// </summary>
        public string WSUrl
        {
            get => this.wSUrl;
            set => this.wSUrl = string.IsNullOrEmpty(value) ? "/" : value;
        }

        /// <summary>
        /// 设置处理WS数据的回调。
        /// </summary>
        /// <param name="action"></param>
        public WebSocketServerPlugin SetCallback(Action<ITcpClientBase, WSDataFrameEventArgs> action)
        {
            this.HandleWSDataFrameCallback = action;
            return this;
        }

        /// <summary>
        /// 用于WebSocket连接的路径，默认为“/ws”
        /// <para>如果设置为null或空，则意味着所有的连接都将解释为WS</para>
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public WebSocketServerPlugin SetWSUrl(string url)
        {
            this.WSUrl = url;
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnGet(ITcpClientBase client, HttpContextEventArgs e)
        {
            if (this.WSUrl == "/" || e.Context.Request.UrlEquals(this.WSUrl))
            {
                if (client.Protocol == Protocol.Http)
                {
                    e.Handled = true;
                    if (client is HttpSocketClient socketClient)
                    {
                        socketClient.SwitchProtocolToWebSocket(e.Context);
                    }
                }
            }
            base.OnGet(client, e);
        }

        /// <summary>
        /// 处理WS数据帧。覆盖父类方法将不会触发<see cref="HandleWSDataFrameCallback"/>回调和插件。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnHandleWSDataFrame(ITcpClientBase client, WSDataFrameEventArgs e)
        {
            this.PluginsManager.Raise<IWebSocketPlugin>("OnHandleWSDataFrame", client, e);
            if (e.Handled)
            {
                return;
            }
            this.HandleWSDataFrameCallback?.Invoke(client, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnReceivedData(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            if (client.Protocol == Protocol.WebSocket)
            {
                if (e.RequestInfo is WSDataFrame dataFrame)
                {
                    e.Handled = true;
                    this.OnHandleWSDataFrame(client, new WSDataFrameEventArgs(dataFrame));
                }
            }
            base.OnReceivedData(client, e);
        }
    }
}