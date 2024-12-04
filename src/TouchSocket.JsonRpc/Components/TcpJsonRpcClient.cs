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
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// 基于Tcp协议的TcpJsonRpc客户端
    /// </summary>
    public class TcpJsonRpcClient : TcpClientBase, ITcpJsonRpcClient
    {
        private readonly WaitHandlePool<IWaitResult> m_waitHandle = new WaitHandlePool<IWaitResult>();
        private IRpcServerProvider m_rpcServerProvider;

        /// <summary>
        /// JsonRpc的调用键。
        /// </summary>
        public ActionMap ActionMap { get; private set; } = new ActionMap(true);

        /// <inheritdoc/>
        public Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            return this.TcpConnectAsync(millisecondsTimeout, token);
        }

        /// <inheritdoc/>
        public async Task<object> InvokeAsync(string invokeKey, Type returnType, IInvokeOption invokeOption, params object[] parameters)
        {
            var context = new JsonRpcWaitResult();
            var waitData = this.m_waitHandle.GetWaitDataAsync(context);
            invokeOption ??= InvokeOption.WaitInvoke;

            parameters ??= new object[0];

            var jsonRpcRequest = new JsonRpcRequest
            {
                Method = invokeKey,
                Params = parameters,
                Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign : 0
            };

            switch (invokeOption.FeedbackType)
            {
                case FeedbackType.OnlySend:
                    {
                        var data = SerializeConvert.JsonSerializeToBytes(jsonRpcRequest);
                        await this.ProtectedSendAsync(new Memory<byte>(data)).ConfigureAwait(false);
                        this.m_waitHandle.Destroy(waitData);
                        return default;
                    }
                case FeedbackType.WaitSend:
                    {
                        var data = SerializeConvert.JsonSerializeToBytes(jsonRpcRequest);
                        await this.ProtectedSendAsync(new Memory<byte>(data)).ConfigureAwait(false);
                        this.m_waitHandle.Destroy(waitData);
                        return default;
                    }
                case FeedbackType.WaitInvoke:
                    {
                        var data = SerializeConvert.JsonSerializeToBytes(jsonRpcRequest);
                        await this.ProtectedSendAsync(new Memory<byte>(data)).ConfigureAwait(false);
                        await waitData.WaitAsync(invokeOption.Timeout).ConfigureAwait(false);
                        var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
                        this.m_waitHandle.Destroy(waitData);

                        if (resultContext.Status == 0)
                        {
                            throw new TimeoutException("等待结果超时");
                        }
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
                            if (returnType != null)
                            {
                                if (returnType.IsPrimitive || returnType == typeof(string))
                                {
                                    return resultContext.Result.ToString().ParseToType(returnType);
                                }
                                else
                                {
                                    return resultContext.Result.ToJsonString().FromJsonString(returnType);
                                }
                            }
                            else
                            {
                                return default;
                            }
                        }
                    }
                default:
                    return default;
            }
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            base.LoadConfig(config);

            var rpcServerProvider = this.Resolver.Resolve<IRpcServerProvider>();
            if (rpcServerProvider != null)
            {
                this.RegisterServer(rpcServerProvider.GetMethods());
                this.m_rpcServerProvider = rpcServerProvider;
            }
        }

        /// <inheritdoc/>
        protected override Task OnTcpReceived(ReceivedDataEventArgs e)
        {
            string jsonString = null;
            if (e.ByteBlock == null)
            {
                if (e.RequestInfo is IJsonRpcRequestInfo jsonRpcRequest)
                {
                    jsonString = jsonRpcRequest.GetJsonRpcString();
                }
            }
            else
            {
                jsonString = e.ByteBlock.ToString();
            }

            if (string.IsNullOrEmpty(jsonString))
            {
                return EasyTask.CompletedTask;
            }

            try
            {
                if (this.ActionMap.Count > 0 && JsonRpcUtility.IsJsonRpcRequest(jsonString))
                {
                    Task.Factory.StartNew(this.ThisInvokeAsync, new WebSocketJsonRpcCallContext(this, jsonString, this.Resolver.CreateScopedResolver()));
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
            }
            catch
            {
            }
            return EasyTask.CompletedTask;
        }

        private void RegisterServer(RpcMethod[] rpcMethods)
        {
            foreach (var rpcMethod in rpcMethods)
            {
                if (rpcMethod.GetAttribute<JsonRpcAttribute>() is JsonRpcAttribute attribute)
                {
                    this.ActionMap.Add(attribute.GetInvokeKey(rpcMethod), rpcMethod);
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
                var data = JsonRpcUtility.ToJsonRpcResponseString(response).ToUTF8Bytes();
                await this.ProtectedSendAsync(new Memory<byte>(data)).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        private async Task ThisInvokeAsync(object obj)
        {
            var callContext = (JsonRpcCallContextBase)obj;
            try
            {
                var invokeResult = new InvokeResult();

                try
                {
                    JsonRpcUtility.BuildRequestContext(this.ActionMap, ref callContext);
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
                    invokeResult = await this.m_rpcServerProvider.ExecuteAsync(callContext, callContext.JsonRpcContext.Parameters).ConfigureAwait(false);
                }

                if (!callContext.JsonRpcContext.Id.HasValue)
                {
                    return;
                }
                var error = JsonRpcUtility.GetJsonRpcError(invokeResult);
                await this.ResponseAsync(callContext, invokeResult.Result, error).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                //这里的异常信息不重要，所以按Debug级别记录
                this.Logger?.Debug(ex.Message);
            }
            finally
            {
                callContext.Dispose();
            }
        }
    }
}