using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// 基于Http协议的JsonRpc客户端
    /// </summary>
    public class HttpJsonRpcClient : HttpClientBase, IHttpJsonRpcClient
    {
        private readonly WaitHandlePool<IWaitResult> m_waitHandle = new WaitHandlePool<IWaitResult>();

        /// <inheritdoc/>
        public object Invoke(Type returnType, string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            var context = new JsonRpcWaitResult();
            var waitData = this.m_waitHandle.GetWaitData(context);

            using (var byteBlock = new ByteBlock())
            {
                if (invokeOption == default)
                {
                    invokeOption = InvokeOption.WaitInvoke;
                }

                parameters ??= new object[0];
                var jsonRpcRequest = new JsonRpcRequest
                {
                    Method = method,
                    Params = parameters,
                    Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign : 0
                };
                var request = new HttpRequest();
                request.Method = HttpMethod.Post;
                request.SetUrl(this.RemoteIPHost.PathAndQuery);
                request.FromJson(jsonRpcRequest.ToJsonString());
                request.Build(byteBlock);
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.Send(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.Send(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.Send(byteBlock);
                            waitData.Wait(invokeOption.Timeout);
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
                                if (returnType.IsPrimitive || returnType == typeof(string))
                                {
                                    return resultContext.Result.ToString().ParseToType(returnType);
                                }
                                else
                                {
                                    return resultContext.Result.ToJsonString().FromJsonString(returnType);
                                }
                            }
                        }
                    default:
                        return default;
                }
            }
        }

        /// <inheritdoc/>
        public void Invoke(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            var context = new JsonRpcWaitResult();
            var waitData = this.m_waitHandle.GetWaitData(context);

            using (var byteBlock = new ByteBlock())
            {
                if (invokeOption == default)
                {
                    invokeOption = InvokeOption.WaitInvoke;
                }
                parameters ??= new object[0];
                var jsonRpcRequest = new JsonRpcRequest()
                {
                    Method = method,
                    Params = parameters
                };

                jsonRpcRequest.Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign : 0;
                var request = new HttpRequest();
                request.Method = HttpMethod.Post;
                request.SetUrl(this.RemoteIPHost.PathAndQuery);
                request.FromJson(jsonRpcRequest.ToJsonString());
                request.Build(byteBlock);
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.Send(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.Send(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.Send(byteBlock);
                            waitData.Wait(invokeOption.Timeout);
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
                            break;
                        }
                    default:
                        return;
                }
            }
        }

        /// <inheritdoc/>
        public void Invoke(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            this.Invoke(method, invokeOption, ref parameters, null);
        }

        /// <inheritdoc/>
        public object Invoke(Type returnType, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return this.Invoke(returnType, method, invokeOption, ref parameters, null);
        }

        /// <inheritdoc/>
        public async Task InvokeAsync(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            var context = new JsonRpcWaitResult();
            var waitData = this.m_waitHandle.GetWaitDataAsync(context);

            using (var byteBlock = new ByteBlock())
            {
                if (invokeOption == default)
                {
                    invokeOption = InvokeOption.WaitInvoke;
                }
                parameters ??= new object[0];
                var jsonRpcRequest = new JsonRpcRequest
                {
                    Method = method,
                    Params = parameters,
                    Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign : 0
                };
                var request = new HttpRequest
                {
                    Method = HttpMethod.Post
                };
                request.SetUrl(this.RemoteIPHost.PathAndQuery);
                request.FromJson(jsonRpcRequest.ToJsonString());
                request.Build(byteBlock);
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.Send(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.Send(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.Send(byteBlock);
                            await waitData.WaitAsync(invokeOption.Timeout);
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
                            break;
                        }
                    default:
                        return;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<object> InvokeAsync(Type returnType, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            var context = new JsonRpcWaitResult();
            var waitData = this.m_waitHandle.GetWaitDataAsync(context);

            using (var byteBlock = new ByteBlock())
            {
                if (invokeOption == default)
                {
                    invokeOption = InvokeOption.WaitInvoke;
                }
                parameters ??= new object[0];
                var jsonRpcRequest = new JsonRpcRequest
                {
                    Method = method,
                    Params = parameters,
                    Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign : 0
                };
                var request = new HttpRequest();
                request.Method = HttpMethod.Post;
                request.SetUrl(this.RemoteIPHost.PathAndQuery);
                request.FromJson(jsonRpcRequest.ToJsonString());
                request.Build(byteBlock);
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                        {
                            this.Send(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return default;
                        }
                    case FeedbackType.WaitSend:
                        {
                            this.Send(byteBlock);
                            this.m_waitHandle.Destroy(waitData);
                            return default;
                        }
                    case FeedbackType.WaitInvoke:
                        {
                            this.Send(byteBlock);
                            await waitData.WaitAsync(invokeOption.Timeout);
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
                                if (returnType.IsPrimitive || returnType == typeof(string))
                                {
                                    return resultContext.Result.ToString().ParseToType(returnType);
                                }
                                else
                                {
                                    return resultContext.Result.ToJsonString().FromJsonString(returnType);
                                }
                            }
                        }
                    default:
                        return default;
                }
            }
        }

        /// <inheritdoc/>
        protected override bool HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            var httpResponse = (HttpResponse)requestInfo;
            var jsonString = httpResponse.GetBody();

            if (string.IsNullOrEmpty(jsonString))
            {
                return base.HandleReceivedData(byteBlock, requestInfo);
            }

            try
            {
                var waitResult = JsonRpcUtility.ToJsonRpcWaitResult(jsonString);
                if (waitResult != null)
                {
                    waitResult.Status = 1;
                    this.m_waitHandle.SetRun(waitResult);
                }
            }
            catch
            {
            }
            return base.HandleReceivedData(byteBlock, requestInfo);
        }
    }
}