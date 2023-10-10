using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Rpc;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// JsonRpcActionClientBase
    /// </summary>
    public abstract class JsonRpcActionClientBase : IJsonRpcActionClient
    {
        private readonly WaitHandlePool<JsonRpcWaitResult> m_waitHandle = new WaitHandlePool<JsonRpcWaitResult>();

        /// <summary>
        /// WaitHandle
        /// </summary>
        public WaitHandlePool<JsonRpcWaitResult> WaitHandle => this.m_waitHandle;

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
                Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign : 0
            };
            try
            {
                this.SendJsonString(jsonRpcRequest.ToJsonString());
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

                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
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
                                    throw new Exception(TouchSocketCoreResource.UnknownError.GetDescription());
                            }
                        }
                }
            }
            finally
            {
                this.m_waitHandle.Destroy(waitData);
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
            var jsonRpcRequest = new JsonRpcRequest
            {
                Method = method,
                Params = parameters,
                Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign : 0
            };
            try
            {
                this.SendJsonString(jsonRpcRequest.ToJsonString());
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                    case FeedbackType.WaitSend:
                        {
                            return;
                        }
                    case FeedbackType.WaitInvoke:
                    default:
                        {
                            if (invokeOption.Token.CanBeCanceled)
                            {
                                waitData.SetCancellationToken(invokeOption.Token);
                            }

                            switch (waitData.Wait(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
                                        if (resultContext.Error != null)
                                        {
                                            throw new RpcException(resultContext.Error.Message);
                                        }
                                        return;
                                    }
                                case WaitDataStatus.Overtime:
                                    throw new TimeoutException("等待结果超时");
                                case WaitDataStatus.Canceled:
                                    return;

                                case WaitDataStatus.Default:
                                case WaitDataStatus.Disposed:
                                default:
                                    throw new Exception(TouchSocketCoreResource.UnknownError.GetDescription());
                            }
                        }
                }
            }
            finally
            {
                this.m_waitHandle.Destroy(waitData);
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
            var jsonRpcRequest = new JsonRpcRequest
            {
                Method = method,
                Params = parameters,
                Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign : 0
            };
            try
            {
                await this.SendJsonStringAsync(jsonRpcRequest.ToJsonString());
                switch (invokeOption.FeedbackType)
                {
                    case FeedbackType.OnlySend:
                    case FeedbackType.WaitSend:
                        {
                            return;
                        }
                    case FeedbackType.WaitInvoke:
                    default:
                        {
                            if (invokeOption.Token.CanBeCanceled)
                            {
                                waitData.SetCancellationToken(invokeOption.Token);
                            }

                            switch (await waitData.WaitAsync(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
                                        if (resultContext.Error != null)
                                        {
                                            throw new RpcException(resultContext.Error.Message);
                                        }
                                        return;
                                    }
                                case WaitDataStatus.Overtime:
                                    throw new TimeoutException("等待结果超时");
                                case WaitDataStatus.Canceled:
                                    return;

                                case WaitDataStatus.Default:
                                case WaitDataStatus.Disposed:
                                default:
                                    throw new Exception(TouchSocketCoreResource.UnknownError.GetDescription());
                            }
                        }
                }
            }
            finally
            {
                this.m_waitHandle.Destroy(waitData);
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
                Id = invokeOption.FeedbackType == FeedbackType.WaitInvoke ? context.Sign : 0
            };
            try
            {
                await this.SendJsonStringAsync(jsonRpcRequest.ToJsonString());
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

                            switch (await waitData.WaitAsync(invokeOption.Timeout))
                            {
                                case WaitDataStatus.SetRunning:
                                    {
                                        var resultContext = (JsonRpcWaitResult)waitData.WaitResult;
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
                                    throw new Exception(TouchSocketCoreResource.UnknownError.GetDescription());
                            }
                        }
                }
            }
            finally
            {
                this.m_waitHandle.Destroy(waitData);
            }
        }

        /// <summary>
        /// 发送Json字符串
        /// </summary>
        /// <param name="jsonString"></param>
        protected abstract void SendJsonString(string jsonString);

        /// <summary>
        /// 发送Json字符串
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        protected abstract Task SendJsonStringAsync(string jsonString);

        /// <inheritdoc/>
        public virtual void InputResponseString(string jsonString)
        {
            var waitResult = JsonRpcUtility.ToJsonRpcWaitResult(jsonString);
            this.m_waitHandle.SetRun(waitResult);
        }
    }
}