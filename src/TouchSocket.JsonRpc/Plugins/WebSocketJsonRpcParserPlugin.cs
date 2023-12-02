//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Rpc;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// WebSocketJsonRpcParserPlugin
    /// </summary>
    [PluginOption(Singleton = true)]
    public sealed class WebSocketJsonRpcParserPlugin : JsonRpcParserPluginBase
    {
        /// <summary>
        /// WebSocketJsonRpcParserPlugin
        /// </summary>
        /// <param name="rpcServerProvider"></param>
        public WebSocketJsonRpcParserPlugin(IRpcServerProvider rpcServerProvider) : base(rpcServerProvider)
        {
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
        protected override void Loaded(IPluginManager pluginManager)
        {
            base.Loaded(pluginManager);
            pluginManager.Add<IHttpSocketClient, HttpContextEventArgs>(nameof(IWebSocketHandshakedPlugin.OnWebSocketHandshaked), this.OnWebSocketHandshaked);

            pluginManager.Add<IHttpSocketClient, WSDataFrameEventArgs>(nameof(IWebSocketReceivedPlugin.OnWebSocketReceived), this.OnWebSocketReceived);
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