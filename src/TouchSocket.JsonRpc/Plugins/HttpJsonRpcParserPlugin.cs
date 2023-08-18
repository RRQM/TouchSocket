using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// HttpJsonRpcParserPlugin
    /// </summary>
    [PluginOption(Singleton = true, NotRegister = false)]
    public sealed class HttpJsonRpcParserPlugin : JsonRpcParserPluginBase, IHttpPostPlugin
    {
        private string m_jsonRpcUrl = "/jsonrpc";

        /// <summary>
        /// HttpJsonRpcParserPlugin
        /// </summary>
        /// <param name="container"></param>
        public HttpJsonRpcParserPlugin(IContainer container) : base(container)
        {
        }

        /// <summary>
        /// 当挂载在<see cref="HttpService"/>时，匹配Url然后响应。当设置为null或空时，会全部响应。
        /// </summary>
        public string JsonRpcUrl
        {
            get => this.m_jsonRpcUrl;
            set => this.m_jsonRpcUrl = string.IsNullOrEmpty(value) ? "/" : value;
        }

        async Task IHttpPostPlugin<IHttpSocketClient>.OnHttpPost(IHttpSocketClient client, HttpContextEventArgs e)
        {
            if (this.m_jsonRpcUrl == "/" || e.Context.Request.UrlEquals(this.m_jsonRpcUrl))
            {
                e.Handled = true;
                this.ThisInvoke(new HttpJsonRpcCallContext(client, e.Context.Request.GetBody(), e.Context));
            }
            else
            {
                await e.InvokeNext();
            }
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
        protected override sealed void Response(JsonRpcCallContextBase callContext, object result, JsonRpcError error)
        {
            try
            {
                using (var responseByteBlock = new ByteBlock())
                {
                    object jobject;
                    if (error == null)
                    {
                        jobject = new JsonRpcSuccessResponse
                        {
                            result = result,
                            id = callContext.JsonRpcContext.Id
                        };
                    }
                    else
                    {
                        jobject = new JsonRpcErrorResponse
                        {
                            error = error,
                            id = callContext.JsonRpcContext.Id
                        };
                    }

                    var httpResponse = ((HttpJsonRpcCallContext)callContext).HttpContext.Response;
                    httpResponse.FromJson(jobject.ToJson());
                    httpResponse.Answer();
                }
            }
            catch
            {
            }
        }
    }
}