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

using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// HttpJsonRpcParserPlugin
    /// </summary>
    [PluginOption(Singleton = true)]
    public sealed class HttpJsonRpcParserPlugin : JsonRpcParserPluginBase
    {
        private string m_jsonRpcUrl = "/jsonrpc";

        /// <summary>
        /// HttpJsonRpcParserPlugin
        /// </summary>
        /// <param name="rpcServerProvider"></param>
        /// <param name="resolver"></param>
        public HttpJsonRpcParserPlugin(IRpcServerProvider rpcServerProvider, IResolver resolver) : base(rpcServerProvider, resolver)
        {
        }

        /// <inheritdoc/>
        protected override void Loaded(IPluginManager pluginManager)
        {
            base.Loaded(pluginManager);
            pluginManager.Add<IHttpSessionClient, HttpContextEventArgs>(typeof(IHttpPlugin), this.OnHttpRequest);
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
        protected override sealed async Task ResponseAsync(JsonRpcCallContextBase callContext, object result, JsonRpcError error)
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
                await httpResponse.AnswerAsync().ConfigureFalseAwait();
            }
            catch
            {
            }
        }

        private async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
        {
            if (e.Context.Request.Method == HttpMethod.Post)
            {
                if (this.m_jsonRpcUrl == "/" || e.Context.Request.UrlEquals(this.m_jsonRpcUrl))
                {
                    e.Handled = true;
                    await this.ThisInvokeAsync(new HttpJsonRpcCallContext(client, e.Context.Request.GetBody(), e.Context)).ConfigureFalseAwait();
                    return;
                }
            }
            await e.InvokeNext().ConfigureFalseAwait();
        }
    }
}