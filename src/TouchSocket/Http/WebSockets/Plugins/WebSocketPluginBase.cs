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
using System.Threading.Tasks;
using TouchSocket.Http.Plugins;
using TouchSocket.Sockets;

namespace TouchSocket.Http.WebSockets.Plugins
{
    /// <summary>
    /// WS插件基类
    /// </summary>
    public class WebSocketPluginBase : HttpPluginBase, IWebSocketPlugin
    {
        #region 虚函数

        /// <summary>
        /// 处理WS数据帧。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnHandleWSDataFrame(ITcpClientBase client, WSDataFrameEventArgs e)
        {
        }

        /// <summary>
        /// 处理WS数据帧。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnHandleWSDataFrameAsync(ITcpClientBase client, WSDataFrameEventArgs e)
        {
            return Task.FromResult(0);
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
        /// 表示完成握手后。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnHandshakedAsync(ITcpClientBase client, HttpContextEventArgs e)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 表示在即将握手连接时。
        /// <para>在此处拒绝操作，则会返回403 Forbidden。</para>
        /// <para>也可以向<see cref="HttpContextEventArgs.Context"/>注入更多信息。</para>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnHandshaking(ITcpClientBase client, HttpContextEventArgs e)
        {
        }

        /// <summary>
        /// 表示在即将握手连接时。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnHandshakingAsync(ITcpClientBase client, HttpContextEventArgs e)
        {
            return Task.FromResult(0);
        }

        #endregion 虚函数

        void IWebSocketPlugin.OnHandleWSDataFrame(ITcpClientBase client, WSDataFrameEventArgs e)
        {
            this.OnHandleWSDataFrame(client, e);
        }

        Task IWebSocketPlugin.OnHandleWSDataFrameAsync(ITcpClientBase client, WSDataFrameEventArgs e)
        {
            return this.OnHandleWSDataFrameAsync(client, e);
        }

        void IWebSocketPlugin.OnHandshaked(ITcpClientBase client, HttpContextEventArgs e)
        {
            this.OnHandshaked(client, e);
        }

        Task IWebSocketPlugin.OnHandshakedAsync(ITcpClientBase client, HttpContextEventArgs e)
        {
            return this.OnHandshakedAsync(client, e);
        }

        void IWebSocketPlugin.OnHandshaking(ITcpClientBase client, HttpContextEventArgs e)
        {
            this.OnHandshaking(client, e);
        }

        Task IWebSocketPlugin.OnHandshakingAsync(ITcpClientBase client, HttpContextEventArgs e)
        {
            return this.OnHandshakingAsync(client, e);
        }
    }
}