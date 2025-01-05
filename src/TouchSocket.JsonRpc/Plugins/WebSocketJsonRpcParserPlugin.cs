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
    public sealed class WebSocketJsonRpcParserPlugin : JsonRpcParserPluginBase, IWebSocketHandshakedPlugin, IWebSocketReceivedPlugin
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
        public Func<IWebSocket, HttpContext, Task<bool>> AllowJsonRpc { get; set; }

        /// <inheritdoc/>
        public async Task OnWebSocketHandshaked(IWebSocket client, HttpContextEventArgs e)
        {
            if (this.AllowJsonRpc != null)
            {
                if (await this.AllowJsonRpc.Invoke(client, e.Context).ConfigureAwait(false))
                {
                    var jsonRpcActor = new JsonRpcActor()
                    {
                        SerializerConverter = this.SerializerConverter,
                        Resolver = client.Client.Resolver,
                        SendAction = (data) => client.SendAsync(data, WSDataType.Text)
                    };

                    jsonRpcActor.SetRpcServerProvider(this.RpcServerProvider, this.ActionMap);
                    client.Client.SetValue(JsonRpcClientExtension.JsonRpcActorProperty, jsonRpcActor);
                }
            }

            await e.InvokeNext().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task OnWebSocketReceived(IWebSocket client, WSDataFrameEventArgs e)
        {
            var dataFrame = e.DataFrame;
            if (dataFrame.Opcode == WSDataType.Text && client.Client.TryGetValue(JsonRpcClientExtension.JsonRpcActorProperty, out var jsonRpcActor))
            {
                e.Handled = true;

                await jsonRpcActor.InputReceiveAsync(dataFrame.PayloadData.Memory, new WebSocketJsonRpcCallContext(client.Client)).ConfigureAwait(false);
            }
            else
            {
                await e.InvokeNext().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 经过判断是否标识当前的客户端为JsonRpc
        /// </summary>
        /// <param name="allowJsonRpc"></param>
        /// <returns></returns>
        public WebSocketJsonRpcParserPlugin SetAllowJsonRpc(Func<IWebSocket, HttpContext, Task<bool>> allowJsonRpc)
        {
            this.AllowJsonRpc = allowJsonRpc;
            return this;
        }

        /// <summary>
        /// 经过判断是否标识当前的客户端为JsonRpc
        /// </summary>
        /// <param name="allowJsonRpc"></param>
        /// <returns></returns>
        public WebSocketJsonRpcParserPlugin SetAllowJsonRpc(Func<IWebSocket, HttpContext, bool> allowJsonRpc)
        {
            this.AllowJsonRpc = (client, context) =>
            {
                return Task.FromResult(allowJsonRpc(client, context));
            };
            return this;
        }


    }
}