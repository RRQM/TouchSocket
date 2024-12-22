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
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http.WebSockets;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// 基于WebSocket协议的JsonRpc客户端。
    /// </summary>
    public class WebSocketJsonRpcClient : SetupClientWebSocket, IWebSocketJsonRpcClient
    {
        private readonly JsonRpcActor m_jsonRpcActor;

        /// <summary>
        /// 初始化 <see cref="WebSocketJsonRpcClient"/> 类的新实例。
        /// </summary>
        public WebSocketJsonRpcClient()
        {
            this.SerializerConverter.Add(new JsonStringToClassSerializerFormatter<JsonRpcActor>());
            this.m_jsonRpcActor = new JsonRpcActor()
            {
                SendAction = this.SendAction,
                SerializerConverter = this.SerializerConverter
            };
        }

        /// <inheritdoc/>
        public TouchSocketSerializerConverter<string, JsonRpcActor> SerializerConverter { get; } = new TouchSocketSerializerConverter<string, JsonRpcActor>();

        #region JsonRpcActor

        private Task SendAction(ReadOnlyMemory<byte> memory)
        {
            var array = memory.GetArray();
            return base.Client.SendAsync(array, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        #endregion JsonRpcActor

        /// <summary>
        /// JsonRpc的调用键。
        /// </summary>
        public ActionMap ActionMap => this.m_jsonRpcActor.ActionMap;

        /// <inheritdoc/>
        public bool Online => base.ProtectedIsHandshaked;

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            base.LoadConfig(config);

            this.m_jsonRpcActor.Logger = this.Logger;
            this.m_jsonRpcActor.Resolver = this.Resolver;
            var rpcServerProvider = this.Resolver.Resolve<IRpcServerProvider>();
            if (rpcServerProvider is not null)
            {
                this.m_jsonRpcActor.SetRpcServerProvider(rpcServerProvider);
            }
        }


        /// <inheritdoc/>
        protected override async Task OnReceived(System.Net.WebSockets.WebSocketReceiveResult result, ByteBlock byteBlock)
        {
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var jsonMemory = byteBlock.Memory;

                if (jsonMemory.IsEmpty)
                {
                    return;
                }

                var callContext = new WebSocketJsonRpcCallContext(this);
                await this.m_jsonRpcActor.InputReceiveAsync(jsonMemory, callContext).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        protected override void OnDisconnected(ClosedEventArgs e)
        {
        }

        /// <inheritdoc/>
        public Task<object> InvokeAsync(string invokeKey, Type returnType, IInvokeOption invokeOption, params object[] parameters)
        {
            return this.m_jsonRpcActor.InvokeAsync(invokeKey, returnType, invokeOption, parameters);
        }
    }
}