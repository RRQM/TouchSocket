using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// WebSocketJsonRpcParserPlugin
    /// </summary>
    [PluginOption(Singleton = true, NotRegister = false)]
    public sealed class WebSocketJsonRpcParserPlugin : JsonRpcParserPluginBase
    {
        /// <summary>
        /// WebSocketJsonRpcParserPlugin
        /// </summary>
        /// <param name="container"></param>
        /// <param name="pluginsManager"></param>
        public WebSocketJsonRpcParserPlugin(IContainer container, IPluginsManager pluginsManager) : base(container)
        {
            pluginsManager.Add<IHttpSocketClient, HttpContextEventArgs>(nameof(IWebSocketHandshakedPlugin.OnWebSocketHandshaked), this.OnWebSocketHandshaked);

            pluginsManager.Add<IHttpSocketClient, WSDataFrameEventArgs>(nameof(IWebSocketReceivedPlugin.OnWebSocketReceived), this.OnWebSocketReceived);
        }

        /// <summary>
        /// 经过判断是否标识当前的客户端为JsonRpc
        /// </summary>
        public Func<IHttpSocketClient, HttpContext, Task<bool>> AllowJsonRpc { get; set; }

        /// <summary>
        /// 经过判断是否标识当前的客户端为JsonRpc
        /// </summary>
        /// <param name="allowJsonRpc"></param>
        /// <returns></returns>
        public WebSocketJsonRpcParserPlugin SetAllowJsonRpc(Func<IHttpSocketClient, HttpContext, Task<bool>> allowJsonRpc)
        {
            this.AllowJsonRpc = allowJsonRpc;
            return this;
        }

        /// <summary>
        /// 经过判断是否标识当前的客户端为JsonRpc
        /// </summary>
        /// <param name="allowJsonRpc"></param>
        /// <returns></returns>
        public WebSocketJsonRpcParserPlugin SetAllowJsonRpc(Func<IHttpSocketClient, HttpContext, bool> allowJsonRpc)
        {
            this.AllowJsonRpc = (client, context) =>
            {
                return Task.FromResult(allowJsonRpc(client, context));
            };
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
                ((IHttpSocketClient)callContext.Caller).SendWithWS(str);
            }
            catch
            {
            }
        }

        private async Task OnWebSocketHandshaked(IHttpSocketClient client, HttpContextEventArgs e)
        {
            if (this.AllowJsonRpc != null)
            {
                if (await this.AllowJsonRpc.Invoke(client, e.Context))
                {
                    client.SetIsJsonRpc();
                }
            }

            await e.InvokeNext();
        }

        private Task OnWebSocketReceived(IHttpSocketClient client, WSDataFrameEventArgs e)
        {
            if (e.DataFrame.Opcode == WSDataType.Text && client.GetIsJsonRpc())
            {
                var jsonRpcStr = e.DataFrame.ToText();
                if (jsonRpcStr.IsNullOrEmpty())
                {
                    return e.InvokeNext();
                }

                e.Handled = true;

                if (client.TryGetValue(JsonRpcClientExtension.JsonRpcActionClientProperty, out var actionClient)
                    && !JsonRpcUtility.IsJsonRpcRequest(jsonRpcStr))
                {
                    actionClient.InputResponseString(jsonRpcStr);
                }
                else
                {
                    Task.Factory.StartNew(this.ThisInvoke, new WebSocketJsonRpcCallContext(client, jsonRpcStr));
                }

                return EasyTask.CompletedTask;
            }
            else
            {
                return e.InvokeNext();
            }
        }
    }
}