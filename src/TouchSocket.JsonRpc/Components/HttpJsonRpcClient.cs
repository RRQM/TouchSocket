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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// 基于Http协议的JsonRpc客户端
    /// </summary>
    public class HttpJsonRpcClient : HttpClientBase, IHttpJsonRpcClient
    {
        private readonly JsonRpcActor m_jsonRpcActor;

        public HttpJsonRpcClient()
        {
            this.SerializerConverter.Add(new JsonStringToClassSerializerFormatter<JsonRpcActor>());
            this.m_jsonRpcActor = new JsonRpcActor()
            {
                SendAction = this.SendAction,
                SerializerConverter = this.SerializerConverter
            };
        }

        public TouchSocketSerializerConverter<string, JsonRpcActor> SerializerConverter { get; } = new TouchSocketSerializerConverter<string, JsonRpcActor>();

        #region JsonRpcActor

        private async Task SendAction(ReadOnlyMemory<byte> memory)
        {
            var request = new HttpRequest();
            request.Method = HttpMethod.Post;
            request.SetUrl(this.RemoteIPHost.PathAndQuery);
            request.SetContent(memory);

            using (var responseResult = await base.ProtectedRequestAsync(request).ConfigureAwait(false))
            {
                var response = responseResult.Response;

                if (response.IsSuccess())
                {
                    await this.m_jsonRpcActor.InputReceiveAsync(await response.GetContentAsync().ConfigureAwait(false), default);
                }
            }
        }

        #endregion JsonRpcActor

        /// <inheritdoc/>
        public Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            return this.TcpConnectAsync(millisecondsTimeout, token);
        }

        /// <inheritdoc/>
        public Task<object> InvokeAsync(string invokeKey, Type returnType, IInvokeOption invokeOption, params object[] parameters)
        {
            return this.m_jsonRpcActor.InvokeAsync(invokeKey, returnType, invokeOption, parameters);
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            base.LoadConfig(config);
            this.m_jsonRpcActor.Logger = this.Logger;
        }
    }
}