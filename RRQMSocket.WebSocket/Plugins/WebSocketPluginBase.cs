using RRQMSocket.Http;
using RRQMSocket.Http.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.WebSocket.Plugins
{
    /// <summary>
    /// WS插件基类
    /// </summary>
    public class WebSocketPluginBase : HttpPluginBase, IWebSocketPlugin
    {

        /// <summary>
        /// 表示在即将握手连接时。
        /// <para>在此处拒绝操作，则会返回403 Forbidden。</para>
        /// <para>也可以向<see cref="HttpContextEventArgs.Response"/>注入更多信息。</para>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnHandshaking(ITcpClientBase client, HttpContextEventArgs e)
        {

        }

        /// <summary>
        /// 表示完成握手后。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnHandshaked(ITcpClientBase client, HttpContextEventArgs e)
        {

        }


        /// <summary>
        /// 处理WS数据帧。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnHandleWSDataFrame(ITcpClientBase client, WSDataFrameEventArgs e)
        {

        }

        void IWebSocketPlugin.OnHandshaking(ITcpClientBase client, HttpContextEventArgs e)
        {
            this.OnHandshaking(client, e);
        }

        void IWebSocketPlugin.OnHandshaked(ITcpClientBase client, HttpContextEventArgs e)
        {
            this.OnHandshaked(client, e);
        }

        void IWebSocketPlugin.OnHandleWSDataFrame(ITcpClientBase client, WSDataFrameEventArgs e)
        {
            this.OnHandleWSDataFrame(client, e);
        }
    }
}
