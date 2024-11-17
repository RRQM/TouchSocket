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
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// 基于Tcp协议的JsonRpc功能插件
    /// </summary>
    [PluginOption(Singleton = true)]
    public sealed class TcpJsonRpcParserPlugin : JsonRpcParserPluginBase
    {
        /// <summary>
        /// 基于Tcp协议的JsonRpc功能插件
        /// </summary>
        /// <param name="rpcServerProvider"></param>
        /// <param name="resolver"></param>
        public TcpJsonRpcParserPlugin(IRpcServerProvider rpcServerProvider, IResolver resolver) : base(rpcServerProvider, resolver)
        {
        }

        /// <inheritdoc/>
        protected override void Loaded(IPluginManager pluginManager)
        {
            base.Loaded(pluginManager);
            pluginManager.Add<ITcpSession, ConnectedEventArgs>(typeof(ITcpConnectedPlugin), this.OnTcpConnected);
            pluginManager.Add<ITcpSession, ReceivedDataEventArgs>(typeof(ITcpReceivedPlugin), this.OnTcpReceived);
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
                if (callContext.Caller is ISender sender)
                {
                    await sender.SendAsync(str.ToUTF8Bytes()).ConfigureAwait(false);
                }
            }
            catch
            {
            }
        }

        private Task OnTcpConnected(ITcpSession client, ConnectedEventArgs e)
        {
            if (this.AutoSwitch && client.Protocol == Protocol.Tcp)
            {
                client.SetIsJsonRpc();
            }

            return e.InvokeNext();
        }

        private Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
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
                        Task.Factory.StartNew(this.ThisInvokeAsync, new WebSocketJsonRpcCallContext(client, jsonRpcStr));
                    }

                    return EasyTask.CompletedTask;
                }
            }

            return e.InvokeNext();
        }
    }
}