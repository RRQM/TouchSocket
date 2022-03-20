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
