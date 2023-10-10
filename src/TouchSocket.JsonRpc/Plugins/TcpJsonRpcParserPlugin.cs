using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// 基于Tcp协议的JsonRpc功能插件
    /// </summary>
    [PluginOption(Singleton = true, NotRegister = false)]
    public sealed class TcpJsonRpcParserPlugin : JsonRpcParserPluginBase
    {
        /// <summary>
        /// 基于Tcp协议的JsonRpc功能插件
        /// </summary>
        /// <param name="container"></param>
        /// <param name="pluginsManager"></param>
        public TcpJsonRpcParserPlugin(IContainer container, IPluginsManager pluginsManager) : base(container)
        {
            pluginsManager.Add<ITcpClientBase, ConnectedEventArgs>(nameof(ITcpConnectedPlugin.OnTcpConnected), OnTcpConnected);
            pluginsManager.Add<ITcpClientBase, ReceivedDataEventArgs>(nameof(ITcpReceivedPlugin.OnTcpReceived), OnTcpReceived);
        }

        /// <summary>
        /// 自动转换协议
        /// </summary>
        public bool AutoSwitch { get; set; } = true;

        /// <summary>
        /// 不需要自动转化协议。
        /// <para>仅当服务器是Tcp时生效。才会解释为jsonRpc。</para>
        /// </summary>
        /// <returns></returns>
        public TcpJsonRpcParserPlugin NoSwitchProtocol()
        {
            this.AutoSwitch = false;
            return this;
        }

        /// <inheritdoc/>
        protected override sealed void Response(JsonRpcCallContextBase callContext, object result, JsonRpcError error)
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
                var client = (ITcpClientBase)callContext.Caller;
                client.Send(str.ToUTF8Bytes());
            }
            catch
            {
            }
        }

        private Task OnTcpConnected(ITcpClientBase client, ConnectedEventArgs e)
        {
            if (this.AutoSwitch && client.Protocol == Protocol.Tcp)
            {
                client.SetIsJsonRpc();
            }

            return e.InvokeNext();
        }

        private Task OnTcpReceived(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            if (client.GetIsJsonRpc())
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
            }

            return e.InvokeNext();
        }
    }
}