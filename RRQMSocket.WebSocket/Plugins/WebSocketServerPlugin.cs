using RRQMCore;
using RRQMCore.ByteManager;
using RRQMCore.Dependency;
using RRQMCore.Run;
using RRQMSocket.Http;
using RRQMSocket.Http.Plugins;
using System;

namespace RRQMSocket.WebSocket
{
    /// <summary>
    /// 基于Http的WebSocket的扩展。
    /// <para>此组件只能挂载在<see cref="HttpService"/>中</para>
    /// </summary>
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
        /// 连接验证超时时间。默认10*1000 ms
        /// </summary>
        public int Timeout { get; set; } = 10 * 1000;

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
        /// 设置连接验证超时时间
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public WebSocketServerPlugin SetTimeout(int timeout)
        {
            this.Timeout = timeout;
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
        /// <para>WS设置连接计时器</para>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnConnected(ITcpClientBase client, RRQMEventArgs e)
        {
            this.CheckHandshaked(client);
            base.OnConnected(client, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnGet(ITcpClientBase client, HttpContextEventArgs e)
        {
            if (this.WSUrl == "/" || e.Request.RelativeURL.Equals(this.WSUrl, StringComparison.OrdinalIgnoreCase))
            {
                if (client.Protocol == Protocol.Http)
                {
                    e.Handled = true;
                    if (WSTools.TryGetResponse(e.Request, out HttpResponse response))
                    {
                        HttpContextEventArgs args = new HttpContextEventArgs(e.Request)
                        {
                            Response = response,
                            IsPermitOperation = true
                        };
                        this.PluginsManager.Raise<IWebSocketPlugin>("OnHandshaking", client, args);

                        if (args.IsPermitOperation)
                        {
                            client.SetDataHandlingAdapter(new WebSocketDataHandlingAdapter() { MaxPackageSize=client.MaxPackageSize});
                            client.Protocol = Protocol.WebSocket;
                            client.SetValue(HandshakedProperty, true);//设置握手状态

                            using (ByteBlock byteBlock = new ByteBlock())
                            {
                                args.Response.Build(byteBlock);
                                client.DefaultSend(byteBlock);
                            }
                            this.PluginsManager.Raise<IWebSocketPlugin>("OnHandshaked", client, new HttpContextEventArgs(e.Request) { Response = response });
                        }
                        else
                        {
                            args.Response.StatusCode = "403";
                            args.Response.StatusMessage = "Forbidden";
                            using (ByteBlock byteBlock = new ByteBlock())
                            {
                                args.Response.Build(byteBlock);
                                client.DefaultSend(byteBlock);
                            }

                            client.Close("主动拒绝WebSocket连接");
                        }
                    }
                    else
                    {
                        client.Close("WebSocket连接协议不正确");
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
            this.PluginsManager.Raise<IWebSocketPlugin>("OnHandleWSDataFrame",client,e);
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

        private void CheckHandshaked(ITcpClientBase client)
        {
            EasyAction.DelayRun(this.Timeout, () =>
            {
                if (!client.GetValue<bool>(HandshakedProperty))
                {
                    client.Close("WebSocket验证超时");
                }
            });
        }
    }
}