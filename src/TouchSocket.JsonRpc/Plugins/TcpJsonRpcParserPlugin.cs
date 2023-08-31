using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// 基于Tcp协议的JsonRpc功能插件
    /// </summary>
    [PluginOption(Singleton = true, NotRegister = false)]
    public sealed class TcpJsonRpcParserPlugin : JsonRpcParserPluginBase, ITcpConnectingPlugin, ITcpReceivedPlugin
    {
        /// <summary>
        /// 基于Tcp协议的JsonRpc功能插件
        /// </summary>
        /// <param name="container"></param>
        public TcpJsonRpcParserPlugin(IContainer container) : base(container)
        {
        }

        /// <summary>
        /// 自动转换协议
        /// </summary>
        public bool AutoSwitch { get; set; } = true;

        /// <summary>
        /// 不需要自动转化协议。
        /// <para>仅当服务器是TCP时生效。此时如果携带协议为TcpJsonRpc时才会解释为jsonRpc。</para>
        /// </summary>
        /// <returns></returns>
        public TcpJsonRpcParserPlugin NoSwitchProtocol()
        {
            this.AutoSwitch = false;
            return this;
        }

        /// <inheritdoc/>
        public Task OnTcpConnecting(ITcpClientBase client, ConnectingEventArgs e)
        {
            if (this.AutoSwitch && client.Protocol == Protocol.Tcp)
            {
                client.Protocol = JsonRpcUtility.TcpJsonRpc;
            }

            return e.InvokeNext();
        }

        /// <inheritdoc/>
        public async Task OnTcpReceived(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            if (client.Protocol == JsonRpcUtility.TcpJsonRpc)
            {
                var jsonRpcStr = string.Empty;
                if (e.RequestInfo is IJsonRpcRequestInfo requestInfo)
                {
                    jsonRpcStr = requestInfo.GetJsonRpcString();
                }
                else if (e.ByteBlock != null)
                {
                    jsonRpcStr = e.ByteBlock.ToString();
                }

                if (jsonRpcStr.HasValue())
                {
                    e.Handled = true;
                    this.ThisInvoke(new TcpJsonRpcCallContext(client, jsonRpcStr));
                    return;
                }
            }

            await e.InvokeNext();
        }

        /// <inheritdoc/>
        protected override sealed void Response(JsonRpcCallContextBase callContext, object result, JsonRpcError error)
        {
            try
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

                var client = (ITcpClientBase)callContext.Caller;
                client.Send(jobject.ToJsonString().ToUTF8Bytes());
            }
            catch
            {
            }
        }
    }
}