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
using System.Security.Cryptography;
using System.Text;
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
        private readonly WaitHandlePool<JsonRpcWaitResult> m_waitHandle = new WaitHandlePool<JsonRpcWaitResult>();
        private IRpcServerProvider m_rpcServerProvider;

        /// <summary>
        /// JsonRpc的调用键。
        /// </summary>
        public ActionMap ActionMap { get; private set; } = new ActionMap(true);

        /// <summary>
        /// WaitHandle
        /// </summary>
        public WaitHandlePool<JsonRpcWaitResult> WaitHandle => this.m_waitHandle;

        /// <inheritdoc/>
        public bool Online => base.ProtectedIsHandshaked;

        private ArraySegment<byte> GetInvokeBytes(JsonRpcRequest request)
        {
            return new ArraySegment<byte>(Encoding.UTF8.GetBytes(request.ToJsonString()));
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            base.LoadConfig(config);
            if (this.Resolver.IsRegistered(typeof(IRpcServerProvider)))
            {
                var rpcServerProvider = this.Resolver.Resolve<IRpcServerProvider>();
                this.RegisterServer(rpcServerProvider.GetMethods());
                this.m_rpcServerProvider = rpcServerProvider;
            }
        }

        private void RegisterServer(RpcMethod[] rpcMethods)
        {
            foreach (var rpcMethod in rpcMethods)
            {
                if (rpcMethod.GetAttribute<JsonRpcAttribute>() is JsonRpcAttribute attribute)
                {
                    this.ActionMap.Add(attribute.GetInvokenKey(rpcMethod), rpcMethod);
                }
            }
        }

        private async Task ResponseAsync(JsonRpcCallContextBase callContext, object result, JsonRpcError error)
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
                await this.Client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(str)), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureFalseAwait();
            }
            catch
            {
            }
        }

        private async Task ThisInvoke(object obj)
        {
            var callContext = (JsonRpcCallContextBase)obj;
            var invokeResult = new InvokeResult();

            try
            {
                JsonRpcUtility.BuildRequestContext(this.Resolver, this.ActionMap, ref callContext);
            }
            catch (Exception ex)
            {
                invokeResult.Status = InvokeStatus.Exception;
                invokeResult.Message = ex.Message;
            }

            if (callContext.RpcMethod != null)
            {
                if (!callContext.RpcMethod.IsEnable)
                {
                    invokeResult.Status = InvokeStatus.UnEnable;
                }
            }
            else
            {
                invokeResult.Status = InvokeStatus.UnFound;
            }

            if (invokeResult.Status == InvokeStatus.Ready)
            {
                invokeResult = await this.m_rpcServerProvider.ExecuteAsync(callContext, callContext.JsonRpcContext.Parameters).ConfigureFalseAwait();
            }

            if (!callContext.JsonRpcContext.Id.HasValue)
            {
                return;
            }
            var error = JsonRpcUtility.GetJsonRpcError(invokeResult);
            await this.ResponseAsync(callContext, invokeResult.Result, error).ConfigureFalseAwait();
        }

        /// <inheritdoc/>
        protected override Task OnReceived(System.Net.WebSockets.WebSocketReceiveResult result, ByteBlock byteBlock)
        {
            if (result.MessageType != WebSocketMessageType.Text)
            {
                return EasyTask.CompletedTask;
            }

            var jsonString = byteBlock.Span.ToString(Encoding.UTF8);
            if (string.IsNullOrEmpty(jsonString))
            {
                return EasyTask.CompletedTask;
            }

            if (this.ActionMap.Count > 0 && JsonRpcUtility.IsJsonRpcRequest(jsonString))
            {
                _ = Task.Factory.StartNew(this.ThisInvoke, new WebSocketJsonRpcCallContext(this, jsonString));
            }
            else
            {
                var waitResult = JsonRpcUtility.ToJsonRpcWaitResult(jsonString);
                if (waitResult != null)
                {
                    waitResult.Status = 1;
                    this.m_waitHandle.SetRun(waitResult);
                }
            }

            return EasyTask.CompletedTask;
        }

        /// <inheritdoc/>
        protected override void OnDisconnected(ClosedEventArgs e)
        {
        }

        public async Task<object> InvokeAsync(string invokeKey, Type returnType, IInvokeOption invokeOption, params object[] parameters)
        {
            var context = new JsonRpcWaitResult();
            var waitData = this.m_waitHandle.GetWaitDataAsync(context);
            invokeOption  ??= InvokeOption.WaitInvoke;

            parameters  ??= new object[0];

            var jsonRpcRequest = new JsonRpcRequest
            {
                Method = invokeKey,
                Params = parameters,
                Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign : 0
            };
            try
            {
                await this.Client.SendAsync(this.GetInvokeBytes(jsonRpcRequest), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureFalseAwait();
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                    case FeedbackType.WaitSend:
                        {
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                    default:
                        {
                            if (invokeOption.Token.CanBeCanceled)
                            {
                                waitData.SetCancellationToken(invokeOption.Token);
                            }

                            switch (await waitData.WaitAsync(invokeOption.Timeout).ConfigureFalseAwait())
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        var resultContext = waitData.WaitResult;
                                        if (resultContext.Error != null)
                                        {
                                            throw new RpcException(resultContext.Error.Message);
                                        }

                                        if (resultContext.Result == null)
                                        {
                                            return default;
                                        }
                                        else
                                        {
                                            return JsonRpcUtility.ResultParseToType(resultContext.Result, returnType);
                                        }
                                    }
                                case WaitDataStatus.Overtime:
                                    throw new TimeoutException("等待结果超时");
                                case WaitDataStatus.Canceled:
                                    return default;

                                case WaitDataStatus.Default:
                                case WaitDataStatus.Disposed:
                                default:
                                    throw new UnknownErrorException();
                            }
                        }
                }
            }
            finally
            {
                this.m_waitHandle.Destroy(waitData);
            }
        }
    }
}