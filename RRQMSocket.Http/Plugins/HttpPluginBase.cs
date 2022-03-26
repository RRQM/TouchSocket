//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Log;
using RRQMSocket.Plugins;

namespace RRQMSocket.Http.Plugins
{
    /// <summary>
    /// Http扩展基类
    /// </summary>
    public class HttpPluginBase: TcpPluginBase, IHttpPlugin
    {
        /// <summary>
        /// <inheritdoc cref="IHttpPlugin.OnReceivedOtherHttpRequest(ITcpClientBase, HttpContextEventArgs)"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnReceivedOtherHttpRequest(ITcpClientBase client, HttpContextEventArgs e)
        {

        }

        /// <summary>
        /// <inheritdoc cref="IHttpPlugin.OnGet(ITcpClientBase, HttpContextEventArgs)"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnGet(ITcpClientBase client, HttpContextEventArgs e)
        {

        }

        /// <summary>
        /// <inheritdoc cref="IHttpPlugin.OnPut(ITcpClientBase, HttpContextEventArgs)"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnPut(ITcpClientBase client, HttpContextEventArgs e)
        {

        }

        /// <summary>
        /// <inheritdoc cref="IHttpPlugin.OnDelete(ITcpClientBase, HttpContextEventArgs)"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnDelete(ITcpClientBase client, HttpContextEventArgs e)
        {

        }

        /// <summary>
        /// <inheritdoc cref="IHttpPlugin.OnPost(ITcpClientBase, HttpContextEventArgs)"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnPost(ITcpClientBase client, HttpContextEventArgs e)
        {

        }

        void IHttpPlugin.OnReceivedOtherHttpRequest(ITcpClientBase client, HttpContextEventArgs e)
        {
            this.OnReceivedOtherHttpRequest(client, e);
        }

        void IHttpPlugin.OnGet(ITcpClientBase client, HttpContextEventArgs e)
        {
            this.OnGet(client, e);
        }

        void IHttpPlugin.OnPut(ITcpClientBase client, HttpContextEventArgs e)
        {
            this.OnPut(client, e);
        }

        void IHttpPlugin.OnDelete(ITcpClientBase client, HttpContextEventArgs e)
        {
            this.OnDelete(client, e);
        }

        void IHttpPlugin.OnPost(ITcpClientBase client, HttpContextEventArgs e)
        {
            this.OnPost(client, e);
        }
    }
}
