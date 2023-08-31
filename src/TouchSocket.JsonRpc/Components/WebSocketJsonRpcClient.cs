using System;
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
    public class WebSocketJsonRpcClient : WebSocketClientBase, IWebSocketJsonRpcClient
    {
        private readonly WaitHandlePool<IWaitResult> m_waitHandle = new WaitHandlePool<IWaitResult>();

        /// <inheritdoc/>
        public object Invoke(Type returnType, string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            var context = new JsonRpcWaitResult();
            var waitData = this.m_waitHandle.GetWaitData(context);

            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }

            parameters ??= new object[0];
            var jsonRpcRequest = new JsonRpcRequest
            {
                Method = method,
                Params = parameters,
                Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign.ToString() : null
            };
            this.SendWithWS(jsonRpcRequest.ToJsonString());
            switch (invokeOption.FeedbackType)
            {
                case FeedbackType.OnlySend:
                case FeedbackType.WaitSend:
                    {
                        this.m_waitHandle.Destroy(waitData);
                        return default;
                    }
                case FeedbackType.WaitInvoke:
                    {
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

                        if (resultContext.Return == null)
                        {
                            return default;
                        }
                        else
                        {
                            if (returnType.IsPrimitive || returnType == typeof(string))
                            {
                                return resultContext.Return.ToString().ParseToType(returnType);
                            }
                            else
                            {
                                return resultContext.Return.ToJsonString().FromJsonString(returnType);
                            }
                        }
                    }
                default:
                    return default;
            }
        }

        /// <inheritdoc/>
        public void Invoke(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            var context = new JsonRpcWaitResult();
            var waitData = this.m_waitHandle.GetWaitData(context);

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

            jsonRpcRequest.Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign.ToString() : null;
            this.SendWithWS(jsonRpcRequest.ToJsonString());
            switch (invokeOption.FeedbackType)
            {
                case FeedbackType.OnlySend:
                case FeedbackType.WaitSend:
                    {
                        this.m_waitHandle.Destroy(waitData);
                        return;
                    }
                case FeedbackType.WaitInvoke:
                    {
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

            jsonRpcRequest.Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign.ToString() : null;
            this.SendWithWS(jsonRpcRequest.ToJsonString());
            switch (invokeOption.FeedbackType)
            {
                case FeedbackType.OnlySend:
                case FeedbackType.WaitSend:
                    {
                        this.m_waitHandle.Destroy(waitData);
                        return;
                    }
                case FeedbackType.WaitInvoke:
                    {
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

        /// <inheritdoc/>
        public async Task<object> InvokeAsync(Type returnType, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            var context = new JsonRpcWaitResult();
            var waitData = this.m_waitHandle.GetWaitDataAsync(context);

            if (invokeOption == default)
            {
                invokeOption = InvokeOption.WaitInvoke;
            }
            parameters ??= new object[0];
            var jsonRpcRequest = new JsonRpcRequest
            {
                Method = method,
                Params = parameters,
                Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign.ToString() : null
            };
            this.SendWithWS(jsonRpcRequest.ToJsonString());
            switch (invokeOption.FeedbackType)
            {
                case FeedbackType.OnlySend:
                case FeedbackType.WaitSend:
                    {
                        this.m_waitHandle.Destroy(waitData);
                        return default;
                    }
                case FeedbackType.WaitInvoke:
                    {
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

                        if (resultContext.Return == null)
                        {
                            return default;
                        }
                        else
                        {
                            if (returnType.IsPrimitive || returnType == typeof(string))
                            {
                                return resultContext.Return.ToString().ParseToType(returnType);
                            }
                            else
                            {
                                return resultContext.Return.ToJsonString().FromJsonString(returnType);
                            }
                        }
                    }
                default:
                    return default;
            }
        }

        /// <inheritdoc/>
        protected override bool HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            string jsonString = null;
            if (requestInfo is WSDataFrame dataFrame && dataFrame.Opcode == WSDataType.Text)
            {
                jsonString = dataFrame.ToText();
            }

            if (string.IsNullOrEmpty(jsonString))
            {
                return base.HandleReceivedData(byteBlock,requestInfo);
            }

            try
            {
                if (jsonString.Contains("error") || jsonString.Contains("result"))
                {
                    var responseContext = jsonString.FromJsonString<JsonResponseContext>();
                    if (responseContext != null && !responseContext.Id.IsNullOrEmpty())
                    {
                        var waitContext = new JsonRpcWaitResult
                        {
                            Status = 1,
                            Sign = long.Parse(responseContext.Id),
                            Error = responseContext.Error,
                            Return = responseContext.Result
                        };
                        this.m_waitHandle.SetRun(waitContext);
                    }
                }
            }
            catch
            {
            }
            return base.HandleReceivedData(byteBlock, requestInfo);
        }
    }
}