using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// HttpJsonRpcParserPlugin
    /// </summary>
    [PluginOption(Singleton = true, NotRegister = false)]
    public sealed class HttpJsonRpcParserPlugin : JsonRpcParserPluginBase
    {
        private string m_jsonRpcUrl = "/jsonrpc";

        /// <summary>
        /// HttpJsonRpcParserPlugin
        /// </summary>
        /// <param name="container"></param>
        /// <param name="pluginsManager"></param>
        public HttpJsonRpcParserPlugin(IContainer container, IPluginsManager pluginsManager) : base(container)
        {
            pluginsManager.Add<IHttpSocketClient, HttpContextEventArgs>(nameof(IHttpPlugin.OnHttpRequest), this.OnHttpRequest);
        }

        /// <summary>
        /// 当挂载在<see cref="HttpService"/>时，匹配Url然后响应。当设置为null或空时，会全部响应。
        /// </summary>
        public string JsonRpcUrl
        {
            get => this.m_jsonRpcUrl;
            set => this.m_jsonRpcUrl = string.IsNullOrEmpty(value) ? "/" : value;
        }

        /// <summary>
        /// 当挂载在<see cref="HttpService"/>时，匹配Url然后响应。当设置为null或空时，会全部响应。
        /// </summary>
        /// <param name="jsonRpcUrl"></param>
        /// <returns></returns>
        public HttpJsonRpcParserPlugin SetJsonRpcUrl(string jsonRpcUrl)
        {
            this.JsonRpcUrl = jsonRpcUrl;
            return this;
        }

        /// <inheritdoc/>
        protected sealed override void Response(JsonRpcCallContextBase callContext, object result, JsonRpcError error)
        {
            try
            {
                JsonRpcResponseBase response;
                if (error == null)
                {
                    response = new JsonRpcSuccessResponse
                    {
                        Result = result,
                        Id = callContext.JsonRpcContext.Id
                    };
                }
                else
                {
                    response = new JsonRpcErrorResponse
                    {
                        Error = error,
                        Id = callContext.JsonRpcContext.Id
                    };
                }
                var str = JsonRpcUtility.ToJsonRpcResponseString(response);

                var httpResponse = ((HttpJsonRpcCallContext)callContext).HttpContext.Response;
                httpResponse.FromJson(str);
                httpResponse.Answer();
            }
            catch
            {
            }
        }

        private async Task OnHttpRequest(IHttpSocketClient client, HttpContextEventArgs e)
        {
            if (e.Context.Request.Method == HttpMethod.Post)
            {
                if (this.m_jsonRpcUrl == "/" || e.Context.Request.UrlEquals(this.m_jsonRpcUrl))
                {
                    e.Handled = true;
                    await this.ThisInvoke(new HttpJsonRpcCallContext(client, e.Context.Request.GetBody(), e.Context));
                    return;
                }
            }
            await e.InvokeNext();
        }
    }
}