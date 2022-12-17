using System;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// 默认的Http服务。为Http做兜底拦截。该插件应该最后添加。
    /// </summary>
    public class DefaultHttpServicePlugin : HttpPluginBase<HttpSocketClient>
    {
        /// <summary>
        /// 默认的Http服务。为Http做兜底拦截。该插件应该最后添加。
        /// </summary>
        public DefaultHttpServicePlugin()
        {
            Order = int.MinValue;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        protected override void OnLoadingConfig(object sender, ConfigEventArgs e)
        {
            if (!(sender is IService))
            {
                throw new Exception("该插件仅可用于服务器。");
            }
            base.OnLoadingConfig(sender, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnGet(HttpSocketClient client, HttpContextEventArgs e)
        {
            e.Context.Response.UrlNotFind().Answer();
            base.OnGet(client, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnPost(HttpSocketClient client, HttpContextEventArgs e)
        {
            e.Context.Response.UrlNotFind().Answer();
            base.OnPost(client, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnPut(HttpSocketClient client, HttpContextEventArgs e)
        {
            e.Context.Response.UrlNotFind().Answer();
            base.OnPut(client, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnDelete(HttpSocketClient client, HttpContextEventArgs e)
        {
            e.Context.Response.UrlNotFind().Answer();
            base.OnDelete(client, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnReceivedOtherHttpRequest(HttpSocketClient client, HttpContextEventArgs e)
        {
            switch (e.Context.Request.Method.ToUpper())
            {
                case "OPTIONS":
                    {
                        e.Context.Response
                            .SetStatus()
                            .SetHeader("Access-Control-Allow-Origin", "*")
                            .SetHeader("Access-Control-Allow-Headers", "*")
                            .SetHeader("Allow", "OPTIONS, GET, POST")
                            .SetHeader("Access-Control-Allow-Methods", "POST, GET, OPTIONS")
                            .Answer();
                        break;
                    }
                default:
                    e.Context.Response.UrlNotFind().Answer();
                    break;
            }
            base.OnReceivedOtherHttpRequest(client, e);
        }
    }
}